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
    /// This class represents a type descriptor creator kind for dictionaries.
    /// A dictionary type implements IDictionary&lt;string, T&gt; from some T.
    /// </summary>
    public class DictionaryDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static DictionaryDescriptorKind Instance { get; } = new DictionaryDescriptorKind();
        private DictionaryDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for a dictionary type.
        /// A dictionary type implements IDictionary&lt;string, T&gt; from some T.
        /// This method returns null if the context does not represent a dictionary tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var valueType = GetValueType(context.Type);
            if (valueType == null)
                return null;
            else
                return TypeDescriptor.MakeDictionary(context.Type.ToString(), creator.GetDescriptor(context.WithType(valueType).WithCutoff(null)));
        }
        private static Type GetValueType(Type type)
        {
            var gendict = type.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GetTypeInfo().GetGenericArguments().Length == 2
                   && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                .Select(i => i.GetGenericArguments())
                .FirstOrDefault();
            if (gendict != null && gendict[0] == typeof(string))
                return gendict[1];
            else if (typeof(IDictionary).IsAssignableFrom(type))
                return typeof(object);
            else
                return null;
        }

        /// <summary>
        /// A dictionary type implements IDictionary&lt;string, T&gt; from some T.
        /// </summary>
        public bool IsOfKind(Type type)
            => GetValueType(type) != null;

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
                ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
                : null;
        private class SerializerImpl<C, T> : Serializer<C>.Typed<T>.Func
            where C : SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> serializer) : base(serializer) { }
            protected override Func<C, SItem, T> MakeDeserializer()
            {
                var valueType = GetValueType(typeof(T));
                if (valueType == typeof(object))
                    return MakeObjectDeserializer();
                else
                    return MakeTypedDeserializer(valueType);
            }

            private Func<C, SItem, T> MakeTypedDeserializer(Type valueType)
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(SItem), "input");
                var obj = Ex.Parameter(typeof(SObject), "obj");
                var prop = Ex.Parameter(typeof(SProperty), "prop");
                var keyLambda = Ex.Lambda(Ex.Property(prop, nameof(SProperty.Name)), prop);
                var valLambda = Ex.Lambda(
                    Ex.Convert(
                        Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { valueType },
                            ctx, Ex.Property(prop, nameof(SProperty.Token))),
                        typeof(object)), prop);
                var block = Ex.Block(new[] { obj },
                    Ex.Assign(obj, Ex.Convert(input, typeof(SObject))),
                    Ex.Call(typeof(Enumerable), nameof(Enumerable.ToDictionary), new[] { typeof(string), valueType },
                        input, keyLambda, valLambda));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            private Func<C, SItem, T> MakeObjectDeserializer()
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(SItem), "input");
                var obj = Ex.Parameter(typeof(SObject), "obj");
                var prop = Ex.Parameter(typeof(SProperty), "prop");
                var keyLambda = Ex.Lambda(Ex.Property(prop, nameof(SProperty.Name)), prop);
                var valLambda = Ex.Lambda(
                    Ex.Convert(
                        Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { typeof(string) },
                            ctx, Ex.Property(prop, nameof(SProperty.Token))),
                        typeof(object)), prop);
                var block = Ex.Block(new[] { obj },
                    Ex.Assign(obj, Ex.Convert(input, typeof(SObject))),
                    Ex.Call(typeof(Enumerable), nameof(Enumerable.ToDictionary), new[] { typeof(string), typeof(object) },
                        input, keyLambda, valLambda));
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            protected override Func<C, T, SItem> MakeSerializer()
            {
                var valueType = GetValueType(typeof(T));
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(T), "input");
                var idx = Ex.Parameter(typeof(int), "idx");
                var result = Ex.Parameter(typeof(SProperty[]), "result");
                var constr = typeof(SProperty).GetConstructor(new[] { typeof(string), typeof(SItem) });
                var block = Ex.Block(new[] { result },
                    Ex.Assign(result,
                        Ex.NewArrayBounds(typeof(SProperty),
                            Ex.Property(Ex.Convert(input, typeof(ICollection<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), valueType))), "Count"))),
                    Ex.Assign(idx, Ex.Constant(0)),
                    input.Foreach(kvp =>
                        Ex.Assign(Ex.ArrayAccess(result, Ex.PostIncrementAssign(idx)),
                            Ex.New(constr,
                                Ex.Property(kvp, "Key"),
                                Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { valueType },
                                    ctx, Ex.Property(kvp, "Value"))))),
                    Ex.New(typeof(SObject).GetConstructor(new[] { typeof(IEnumerable<SItem>) }), result));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
        }
    }
}
