using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Biz.Morsink.Rest.Serialization;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for unions.
    /// A union type is an abstract class containing nested 'case' classes that derive from the abstract class.
    /// </summary>
    public class UnionDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static UnionDescriptorKind Instance { get; } = new UnionDescriptorKind();
        private UnionDescriptorKind() { }
        private static IEnumerable<Type> GetNestedTypes(Type type)
        {
            var generics = type.GetGenericArguments();
            var nested = generics.Length == 0 ? type.GetNestedTypes() : type.GetNestedTypes().Select(nt => nt.MakeGenericType(generics));
            return nested.SelectMany(nt => IsOfKind(nt) ? GetNestedTypes(nt) : new[] { nt });
        }
        /// <summary>
        /// Gets a type descriptor for a union type.
        /// A union type is an abstract class containing nested 'case' classes that derive from the abstract class.
        /// This method returns null if the context does not represent a union tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (!IsOfKind(context.Type))
                return null;
            var ti = context.Type.GetTypeInfo();
            var rec = RecordDescriptorKind.Instance.GetDescriptor(creator, context);
            var options = GetOptionsForType(ti).Select(ty => creator.GetReferableDescriptor(context.WithType(ty).WithCutoff(context.Type)));
            TypeDescriptor res = new TypeDescriptor.Union(
                rec == null ? context.Type.ToString() : "",
                options,
                rec == null ? context.Type : null);

            if (rec != null)
                res = new TypeDescriptor.Intersection(context.Type.ToString(), new[] { rec, res }, context.Type);

            return res;
        }
        public static IEnumerable<Type> GetOptionsForType(Type baseType)
            => GetNestedTypes(baseType).Where(ty => baseType.IsAssignableFrom(ty.BaseType));
        public static bool IsOfKind(Type type)
        {
            var ti = type.GetTypeInfo();
            return ti.IsAbstract && GetNestedTypes(ti).Any(nt => nt.BaseType == type);
        }
        bool TypeDescriptorCreator.IKind.IsOfKind(Type type)
            => IsOfKind(type);

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
            ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
            : null;
        private class SerializerImpl<C,T> : Serializer<C>.Typed<T>.Func
            where C:SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> parent) : base(parent) { }

            protected override Func<C, SItem, T> MakeDeserializer()
                => (ctx,item) => throw new NotSupportedException();

            protected override Func<C, T, SItem> MakeSerializer()
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(T), "input");
                var options = GetOptionsForType(typeof(T)).Select((ot, idx) => (Ex.Parameter(ot, $"opt{idx}"), RecordDescriptorKind.Instance.GetSerializer(Parent, ot))).ToArray();
                var end = Ex.Label(typeof(SItem), "end");
                var block = Ex.Block(options.Select(o => o.Item1),
                    Ex.Block(options.Select(opt => Ex.Block(
                        Ex.Assign(opt.Item1, Ex.TypeAs(input, opt.Item1.Type)),
                        Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.NotEqual, opt.Item1, Ex.Default(opt.Item1.Type)),
                            Ex.Goto(end, Ex.Call(Ex.Constant(opt.Item2, typeof(Serializer<>.Typed<>).MakeGenericType(typeof(C), opt.Item1.Type)), SERIALIZE, Type.EmptyTypes,
                                ctx, opt.Item1)))))),
                    Ex.Label(end, Ex.Default(typeof(SItem))));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
        }
    }
}
