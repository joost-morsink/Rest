using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Biz.Morsink.Rest.Serialization;
using Biz.Morsink.Rest.Utils;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for collections (arrays).
    /// A collection type implements IEnumerable, and ideally IEnumerable&lt;T&gt; for some T.
    /// </summary>
    public class ArrayDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static ArrayDescriptorKind Instance { get; } = new ArrayDescriptorKind();
        private ArrayDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for a collection type.
        /// A collection type implements IEnumerable, and ideally IEnumerable&lt;T&gt; for some T.
        /// This method returns null if the context does not represent a collection tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var elementType = GetElementType(context.Type);
            if (elementType == null)
                return null;
            else
            {
                var inner = creator.GetReferableDescriptor(context.WithType(elementType).WithCutoff(null));
                return TypeDescriptor.MakeArray(inner);
            }
        }
        private static Type GetElementType(Type type)
        {
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                var q = from itf in type.GetTypeInfo().ImplementedInterfaces.Concat(new[] { type })
                        let iti = itf.GetTypeInfo()
                        let ga = iti.GetGenericArguments()
                        where ga.Length == 1 && iti.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        select ga[0];
                return q.FirstOrDefault() ?? typeof(object);
            }
            else
                return null;
        }
        /// <summary>
        /// A collection type implements IEnumerable, and ideally IEnumerable&lt;T&gt; for some T.
        /// </summary>
        public bool IsOfKind(Type type)
            => GetElementType(type) != null
                && type.GetGenerics2(typeof(IDictionary<,>)).Item1 == null
                && type.GetGenerics2(typeof(IReadOnlyDictionary<,>)).Item1 == null;

        private static (FieldInfo, MethodInfo) GetImmutableCollectionInterface(Type type)
        {
            var fi = type.GetField("Empty", BindingFlags.Public | BindingFlags.Static);
            if (fi == null)
                return default;
            var meth = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Add" && m.ReturnType == type && m.GetParameters().Length == 1)
                .FirstOrDefault();
            if (meth == null)
                return default;
            return (fi, meth);
        }
        private static (PropertyInfo, MethodInfo) GetImmutableCollectionInterface2(Type type)
        {
            var pi = type.GetProperty("Empty", BindingFlags.Public | BindingFlags.Static);
            if (pi == null)
                return default;
            var meth = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => (m.Name == "Enqueue" || m.Name == "Push") && m.ReturnType == type && m.GetParameters().Length == 1)
                .FirstOrDefault();
            if (meth == null)
                return default;
            return (pi, meth);
        }

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
                ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
                : null;

        private class SerializerImpl<C, T> : Serializer<C>.Typed<T>.Func
            where C : SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> parent) : base(parent) { }

            protected override Func<C, SItem, T> MakeDeserializer()
            {
                var eType = GetElementType(typeof(T));
                var enumConstr = typeof(T).GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(eType) });
                var lessConstr = typeof(T).GetConstructor(Type.EmptyTypes);
                if (typeof(T).IsArray
                    || !typeof(ICollection<>).MakeGenericType(eType).IsAssignableFrom(typeof(T)) && typeof(T).IsAssignableFrom(eType.MakeArrayType()))
                    return MakeArrayDeserializer(eType);
                else if (enumConstr != null)
                    return MakeEnumConstDeserializer(eType, enumConstr);
                else if (lessConstr != null && typeof(ICollection<>).MakeGenericType(eType).IsAssignableFrom(typeof(T)))
                    return MakeLessConstrDeserializer(eType, lessConstr);
                else
                {
                    var (fi, meth) = GetImmutableCollectionInterface(typeof(T));
                    var (pi, meth2) = GetImmutableCollectionInterface2(typeof(T));
                    if (fi != null && meth != null)
                        return MakeImmutableDeserializer(eType, fi, meth);
                    else if (pi != null && meth2 != null)
                        return MakeImmutableDeserializer2(eType, pi, meth2);
                    else
                        throw new RestSerializationException($"{typeof(T)} does not support deserialization as a collection.");
                }
            }
            private Func<C, SItem, T> MakeImmutableDeserializer(Type eType, FieldInfo empty, MethodInfo add)
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var item = Ex.Parameter(typeof(SItem), "item");
                var arr = Ex.Parameter(typeof(SArray), "arr");
                var result = Ex.Parameter(typeof(T), "result");
                var block = Ex.Block(new[] { arr, result },
                    Ex.Assign(arr, Ex.Convert(item, typeof(SArray))),
                    Ex.Assign(result, Ex.Field(null, empty)),
                    Ex.Property(arr, nameof(SArray.Content)).Foreach(i =>
                        Ex.Assign(result,
                            Ex.Call(result, add,
                                Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { eType },
                                    ctx,
                                    i)))),
                    result);
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, item);
                return lambda.Compile();
            }
            private Func<C, SItem, T> MakeImmutableDeserializer2(Type eType, PropertyInfo empty, MethodInfo add)
            {
                var reverse = typeof(T).GetGenericTypeDefinition() == typeof(System.Collections.Immutable.ImmutableStack<>);

                var ctx = Ex.Parameter(typeof(C), "ctx");
                var item = Ex.Parameter(typeof(SItem), "item");
                var arr = Ex.Parameter(typeof(SArray), "arr");
                var result = Ex.Parameter(typeof(T), "result");
                var block = Ex.Block(new[] { arr, result },
                    Ex.Assign(arr, Ex.Convert(item, typeof(SArray))),
                    Ex.Assign(result, Ex.Property(null, empty)),
                    (reverse
                        ? (Ex)Ex.Call(typeof(Enumerable), nameof(Enumerable.Reverse), new[] { typeof(SItem) }, Ex.Property(arr, nameof(SArray.Content)))
                        : Ex.Property(arr, nameof(SArray.Content))
                    ).Foreach(i =>
                        Ex.Assign(result,
                            Ex.Call(result, add,
                                Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { eType },
                                    ctx,
                                    i)))),
                    result);
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, item);
                return lambda.Compile();
            }
            private Func<C, SItem, T> MakeLessConstrDeserializer(Type eType, ConstructorInfo constr)
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var item = Ex.Parameter(typeof(SItem), "item");
                var arr = Ex.Parameter(typeof(SArray), "arr");
                var result = Ex.Parameter(typeof(T), "result");
                var block = Ex.Block(new[] { arr, result },
                    Ex.Assign(arr, Ex.Convert(item, typeof(SArray))),
                    Ex.Assign(result, Ex.New(constr)),
                    Ex.Property(arr, nameof(SArray.Content)).Foreach(el =>
                        Ex.Call(Ex.Convert(result, typeof(ICollection<>).MakeGenericType(eType)), "Add", Type.EmptyTypes,
                            Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { eType },
                                ctx, el))),
                    result);

                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, item);
                return lambda.Compile();
            }

            private Func<C, SItem, T> MakeEnumConstDeserializer(Type eType, ConstructorInfo constr)
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var item = Ex.Parameter(typeof(SItem), "item");
                var arr = Ex.Parameter(typeof(SArray), "arr");
                var result = Ex.Parameter(eType.MakeArrayType(), "result");
                var idx = Ex.Parameter(typeof(int), "idx");
                var block = Ex.Block(new[] { arr, idx, result },
                    Ex.Assign(arr, Ex.Convert(item, typeof(SArray))),
                    Ex.Assign(result, Ex.NewArrayBounds(eType, Ex.Property(
                        Ex.Convert(
                            Ex.Property(arr, nameof(SArray.Content)),
                            typeof(IReadOnlyCollection<SItem>)),
                        "Count"))),
                    Ex.Assign(idx, Ex.Constant(0)),
                    Ex.Property(arr, nameof(SArray.Content)).Foreach(el =>
                        Ex.Assign(Ex.ArrayAccess(result, Ex.PostIncrementAssign(idx)),
                            Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { eType },
                                ctx, el))),
                    Ex.New(constr, result));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, item);
                return lambda.Compile();
            }

            private Func<C, SItem, T> MakeArrayDeserializer(Type eType)
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var item = Ex.Parameter(typeof(SItem), "item");
                var arr = Ex.Parameter(typeof(SArray), "arr");
                var result = Ex.Parameter(eType.MakeArrayType(), "result");
                var idx = Ex.Parameter(typeof(int), "idx");
                var block = Ex.Block(new[] { arr, idx, result },
                    Ex.Assign(arr, Ex.Convert(item, typeof(SArray))),
                    Ex.Assign(result, Ex.NewArrayBounds(eType, Ex.Property(
                        Ex.Convert(
                            Ex.Property(arr, nameof(SArray.Content)),
                            typeof(IReadOnlyCollection<SItem>)),
                        "Count"))),
                    Ex.Assign(idx, Ex.Constant(0)),
                    Ex.Property(arr, nameof(SArray.Content)).Foreach(el =>
                        Ex.Assign(Ex.ArrayAccess(result, Ex.PostIncrementAssign(idx)),
                            Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { eType },
                                ctx, el))),
                    Ex.Convert(result, typeof(T)));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, item);
                return lambda.Compile();
            }

            protected override Func<C, T, SItem> MakeSerializer()
            {
                var eType = GetElementType(typeof(T));
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(T), "input");
                var result = Ex.Parameter(typeof(List<SItem>), "result");
                var block = Ex.Block(new[] { result },
                    Ex.Assign(result, Ex.New(typeof(List<SItem>))),
                    input.Foreach(item =>
                        Ex.Call(result, nameof(List<SItem>.Add), Type.EmptyTypes,
                            Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { eType },
                                ctx, item))),
                    Ex.New(typeof(SArray).GetConstructor(new[] { typeof(IEnumerable<SItem>) }), result));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
        }
    }
}
