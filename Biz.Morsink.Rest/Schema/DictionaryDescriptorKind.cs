using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var valueType = GetValueType(context.Type);
            if (valueType == null)
                return null;
            else
                return TypeDescriptor.MakeDictionary(context.Type.ToString(), creator.GetDescriptor(context.WithType(valueType).WithCutoff(null)));
        }
        private static Type GetValueType(Type type)
        {
            var gendict = type.GetTypeInfo().ImplementedInterfaces.Prepend(type)
                .Where(i => i.GetTypeInfo().GenericTypeArguments.Length == 2
                   && (i.GetGenericTypeDefinition() == typeof(IDictionary<,>) || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) || i.GetGenericTypeDefinition() == typeof(IImmutableDictionary<,>)))
                .Select(i => i.GenericTypeArguments)
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
        public static bool IsOfKind(Type type)
            => GetValueType(type) != null && (IsDictionary(type) || HasParameterlessConstructor(type))
                || IsImmutableDictionary(type);

        /// <summary>
        /// Checks if the type is either exactly an IDictionary&lt;string, T&gt; or Dictionary&lt;string, T&gt;.
        /// </summary>
        public static bool IsDictionary(Type type)
            => type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary<,>) || type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
                && type.GetGenericArguments()[0] == typeof(string);

        private static bool HasParameterlessConstructor(Type type)
            => type.GetConstructor(Type.EmptyTypes) != null;
        /// <summary>
        /// Checks if the type is an immutable dictionary with a string key type.
        /// </summary>
        public static bool IsImmutableDictionary(Type type)
            => type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>)) && type.GetGenericArguments()[0] == typeof(string);

        bool TypeDescriptorCreator.IKind.IsOfKind(Type type) => IsOfKind(type);
        /// <summary>
        /// Gets a serializer for a dictionary type.
        /// </summary>
        /// <typeparam name="C">The serialization context type.</typeparam>
        /// <param name="serializer">A parent serializer.</param>
        /// <param name="type">The type to get a serializer for.</param>
        /// <returns>A serializer for the specified type if one could be constructed, null otherwise.</returns>
        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
                ? IsImmutableDictionary(type)
                    ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(ImmSerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
                    : (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
                : null;
        #region Serializer implementation
        private class ImmSerializerImpl<C, T> : Serializer<C>.Typed<T>.Func
            where C : SerializationContext<C>
        {
            public ImmSerializerImpl(Serializer<C> serializer) : base(serializer) { }
            protected override Func<C, SItem, T> MakeDeserializer()
            {
                var valueType = GetValueType(typeof(T));
                if (valueType == typeof(object))
                    return MakeObjectDeserializer();
                else
                    return MakeRegularDeserializer(valueType);
            }
            private Func<C, SItem, T> MakeRegularDeserializer(Type valueType)
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(SItem), "input");
                var res = Ex.Parameter(typeof(T), "res");
                var block = Ex.Block(new[] { res },
                    Ex.Assign(res, Ex.Field(null, typeof(T).GetField("Empty", BindingFlags.Public | BindingFlags.Static))),
                    Ex.Property(Ex.Convert(input, typeof(SObject)), nameof(SObject.Properties)).Foreach(prop =>
                        Ex.Assign(res,
                            Ex.Call(res, nameof(ImmutableDictionary<string, object>.Add), Type.EmptyTypes,
                                Ex.Property(prop, nameof(SProperty.Name)),
                                Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { valueType },
                                    ctx, Ex.Property(prop, nameof(SProperty.Token)))))),
                    res);
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }
            private Func<C, SItem, T> MakeObjectDeserializer()
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(SItem), "input");
                var res = Ex.Parameter(typeof(T), "res");
                var block = Ex.Block(new[] { res },
                    Ex.Assign(res, Ex.Field(null, typeof(T).GetField("Empty", BindingFlags.Public | BindingFlags.Static))),
                    Ex.Property(Ex.Convert(input, typeof(SObject)), nameof(SObject.Properties)).Foreach(prop =>
                        Ex.Assign(res,
                            Ex.Call(res, nameof(ImmutableDictionary<string, object>.Add), Type.EmptyTypes,
                                Ex.Property(prop, nameof(SProperty.Name)),
                                Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { typeof(string) },
                                    ctx, Ex.Property(prop, nameof(SProperty.Token)))))),
                    res);
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }
            protected override Func<C, T, SItem> MakeSerializer()
            {
                var valueType = GetValueType(typeof(T));
                var sprop = typeof(SProperty).GetConstructor(new[] { typeof(string), typeof(SItem) });
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(T), "input");
                var res = Ex.Parameter(typeof(List<SProperty>), "res");
                var block = Ex.Block(new[] { res },
                    Ex.Assign(res, Ex.New(typeof(List<SProperty>))),
                    input.Foreach(kvp =>
                        Ex.Call(res, nameof(List<SProperty>.Add), Type.EmptyTypes,
                            Ex.New(sprop,
                                Ex.Property(kvp, nameof(KeyValuePair<string, object>.Key)),
                                Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { valueType },
                                    ctx,
                                    Ex.Property(kvp, nameof(KeyValuePair<string, object>.Value)))))),
                    Ex.New(typeof(SObject).GetConstructor(new[] { typeof(IEnumerable<SProperty>) }), res));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();
            }
        }
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
                var res = Ex.Parameter(typeof(T), "res");
                var keyLambda = Ex.Lambda(Ex.Property(prop, nameof(SProperty.Name)), prop);
                var valLambda = Ex.Lambda(
                    Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { valueType },
                        ctx, Ex.Property(prop, nameof(SProperty.Token))),
                    prop);
                Ex block;

                if (IsDictionary(typeof(T)))
                    block = Ex.Block(new[] { obj },
                        Ex.Assign(obj, Ex.Convert(input, typeof(SObject))),
                        Ex.Call(typeof(Enumerable), nameof(Enumerable.ToDictionary), new[] { typeof(SProperty), typeof(string), valueType },
                            Ex.Property(obj, nameof(SObject.Properties)), keyLambda, valLambda));
                else
                    block = Ex.Block(new[] { obj, res },
                        Ex.Assign(obj, Ex.Convert(input, typeof(SObject))),
                        Ex.Assign(res, Ex.New(typeof(T))),
                        Ex.Property(obj, nameof(SObject.Properties)).Foreach(sprop =>
                            Ex.Call(Ex.Convert(res, typeof(IDictionary<,>).MakeGenericType(typeof(string), valueType)),
                                nameof(IDictionary<string, object>.Add), Type.EmptyTypes,
                                Ex.Property(sprop, nameof(SProperty.Name)),
                                Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { valueType },
                                    ctx, Ex.Property(sprop, nameof(SProperty.Token))))),
                        res);
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            private Func<C, SItem, T> MakeObjectDeserializer()
            {
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(SItem), "input");
                var obj = Ex.Parameter(typeof(SObject), "obj");
                var prop = Ex.Parameter(typeof(SProperty), "prop");
                var res = Ex.Parameter(typeof(T), "res");
                var keyLambda = Ex.Lambda(Ex.Property(prop, nameof(SProperty.Name)), prop);
                var valLambda = Ex.Lambda(
                    Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { typeof(string) },
                        ctx, Ex.Property(prop, nameof(SProperty.Token))),
                    prop);
                Ex block;
                if (IsDictionary(typeof(T)))
                    block = Ex.Block(new[] { obj },
                    Ex.Assign(obj, Ex.Convert(input, typeof(SObject))),
                    Ex.Call(typeof(Enumerable), nameof(Enumerable.ToDictionary), new[] { typeof(SProperty), typeof(string), typeof(object) },
                        Ex.Property(obj, nameof(SObject.Properties)), keyLambda, valLambda));
                else
                    block = Ex.Block(new[] { obj, res },
                        Ex.Assign(obj, Ex.Convert(input, typeof(SObject))),
                        Ex.Assign(res, Ex.New(typeof(T))),
                        Ex.Property(obj, nameof(SObject.Properties)).Foreach(sprop =>
                            Ex.Call(Ex.Convert(res, typeof(IDictionary<string, object>)),
                                nameof(IDictionary<string, string>.Add), Type.EmptyTypes,
                                Ex.Property(sprop, nameof(SProperty.Name)),
                                Ex.Call(Ex.Constant(Parent), DESERIALIZE, new[] { typeof(string) },
                                    ctx, Ex.Property(sprop, nameof(SProperty.Token))))),
                        res);
                var lambda = Ex.Lambda<Func<C, SItem, T>>(block, ctx, input);
                return lambda.Compile();
            }

            protected override Func<C, T, SItem> MakeSerializer()
            {
                var valueType = GetValueType(typeof(T));
                var attr = typeof(T).GetCustomAttribute<SFormatAttribute>();
                var ctx = Ex.Parameter(typeof(C), "ctx");
                var input = Ex.Parameter(typeof(T), "input");
                var idx = Ex.Parameter(typeof(int), "idx");
                var result = Ex.Parameter(typeof(SProperty[]), "result");
                var constr = typeof(SProperty).GetConstructor(new[] { typeof(string), typeof(SItem) });
                var constr2 = typeof(SProperty).GetConstructor(new[] { typeof(string), typeof(SItem), typeof(SFormat) });
                var block = Ex.Block(new[] { result, idx },
                    Ex.Assign(result,
                        Ex.NewArrayBounds(typeof(SProperty),
                            Ex.Property(Ex.Convert(input, typeof(ICollection<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), valueType))), "Count"))),
                    Ex.Assign(idx, Ex.Constant(0)),
                    input.Foreach(kvp =>
                        Ex.Assign(Ex.ArrayAccess(result, Ex.PostIncrementAssign(idx)),
                            handleKvp(kvp))),
                    Ex.New(typeof(SObject).GetConstructor(new[] { typeof(IEnumerable<SProperty>) }), result));
                var lambda = Ex.Lambda<Func<C, T, SItem>>(block, ctx, input);
                return lambda.Compile();

                Ex handleKvp(Ex kvp)
                {
                    if (attr != null)
                        return Ex.New(constr2,
                            Ex.Property(kvp, "Key"),
                            Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { valueType },
                                ctx, Ex.Property(kvp, "Value")),
                            Ex.Constant(attr.Property));
                    else
                        return Ex.New(constr,
                            Ex.Property(kvp, "Key"),
                            Ex.Call(Ex.Constant(Parent), SERIALIZE, new[] { valueType },
                                ctx, Ex.Property(kvp, "Value")));
                }
            }
        }
        #endregion
    }
}
