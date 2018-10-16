using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A Type representation for tuples as intersections.
    /// </summary>
    public class TupleAsIntersectionRepresentation : ITypeRepresentation
    {
        /// <summary>
        /// A singleton property for TupleAsIntersectionRepresentation.
        /// </summary>
        public static TupleAsIntersectionRepresentation Instance { get; } = new TupleAsIntersectionRepresentation();

        private ConcurrentDictionary<Type, Func<object, object[]>> getValueDict = new ConcurrentDictionary<Type, Func<object, object[]>>();
        private ConcurrentDictionary<Type, Type[]> getTypeDict = new ConcurrentDictionary<Type, Type[]>();

        private Type[] GetTypes(Type type)
            => IsTuple(type)
                ? getTypeDict.GetOrAdd(type, (t) => t.GetGenericArguments())
                : null;
        private Func<object, object[]> GetValuesFunction(Type type)
            => IsTuple(type)
                ? getValueDict.GetOrAdd(type, CreateValuesFunction)
                : null;

        private Func<object, object[]> CreateValuesFunction(Type type)
        {
            var props = type.GetProperties().Where(prop => prop.Name.StartsWith("Item")).OrderBy(prop => prop.Name).ToArray();
            var input = Ex.Parameter(typeof(object), "input");
            var tuple = Ex.Parameter(type, "tuple");
            var result = Ex.Parameter(typeof(object[]), "result");

            var block = Ex.Block(new[] { tuple, result },
                Ex.Assign(result, Ex.NewArrayBounds(typeof(object), Ex.Constant(props.Length))),
                Ex.Assign(tuple, Ex.Convert(input, type)),
                Ex.Block(props.Select((prop, idx) => Ex.Assign(Ex.ArrayAccess(result, Ex.Constant(idx)), Ex.Property(tuple, prop)))),
                result);
            var lambda = Ex.Lambda<Func<object, object[]>>(block, input);
            return lambda.Compile();
        }

        /// <summary>
        /// Checks whether the specified type is a Tuple type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the specified type is a Tuple type, false otherwise.</returns>
        public static bool IsTuple(Type type)
            => type.Namespace == "System" && type.Name.StartsWith("Tuple`");

        public object GetRepresentable(object rep, Type specific)
        {
            var subtypes = GetTypes(specific);
            var irep = (IntersectionRepresentation)rep;
            var values = irep.GetValues();
            return Activator.CreateInstance(specific, values.ToArray());
        }
        public Type GetRepresentableType(Type type)
            => typeof(object);

        public object GetRepresentation(object obj)
        {
            if (obj == null)
                return null;
            var type = obj.GetType();
            if (!IsRepresentable(type))
                return null;
            var vals = GetValuesFunction(type)(obj);
            var types = GetTypes(type);
            return types.Zip(vals, (typ, value) => (type: typ, value))
                .Aggregate(IntersectionRepresentation.Create(), (ir, tup) => ir.Add(tup.type, tup.value))
                .Create();
        }

        public Type GetRepresentationType(Type type)
            => IsTuple(type)
                ? typeof(IntersectionRepresentation).Assembly.GetTypes()
                    .Where(ty => ty.Namespace == typeof(IntersectionRepresentation).Namespace 
                            && ty.Name.StartsWith(nameof(IntersectionRepresentation) + "`"))
                    .Select(ty => ty.MakeGenericType(GetTypes(type)))
                    .FirstOrDefault()
                : null;

        public bool IsRepresentable(Type type)
            => GetRepresentationType(type) != null;
        public bool IsRepresentation(Type type)
            => typeof(IntersectionRepresentation).IsAssignableFrom(type);
    }
}
