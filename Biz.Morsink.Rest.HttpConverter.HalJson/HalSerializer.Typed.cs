using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using Newtonsoft.Json.Linq;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {
        private static string StripName(string name)
        {
            if (name.Contains('`'))
                return name.Substring(0, name.IndexOf('`'));
            else
                return name;
        }

        /// <summary>
        /// Abstract base class for serializers that handle a specific single type.
        /// </summary>
        /// <typeparam name="T">The type the serializer handles.</typeparam>
        public abstract class Typed<T> : IForType
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parent">A reference to the parent HalSerializer instance.</param>
            protected Typed(HalSerializer parent)
            {
                Parent = parent;
            }
            /// <summary>
            /// Should implement serialization for objects of type T.
            /// </summary>
            /// <param name="context">The applicable HalContext.</param>
            /// <param name="item">The object to serialize.</param>
            /// <returns>The serialization of the object as a JToken.</returns>
            public abstract JToken Serialize(HalContext context, T item);
            /// <summary>
            /// Should implement deserialization to objects of type T.
            /// </summary>
            /// <param name="context">The applicable HalContext.</param>
            /// <param name="token">The JToken to deserialize.</param>
            /// <returns>A deserialized object of type T.</returns>
            public abstract T Deserialize(HalContext context, JToken token);

            Type IForType.Type => typeof(T);
            JToken IForType.Serialize(HalContext context, object item) => Serialize(context, (T)item);
            object IForType.Deserialize(HalContext context, JToken token) => Deserialize(context, token);

            /// <summary>
            /// Contains a reference to the parent HalSerializer instance.
            /// </summary>
            protected HalSerializer Parent { get; }

            /// <summary>
            /// Typed HalSerializer for simple (primitive) types.
            /// </summary>
            public class Simple : Typed<T>
            {
                private readonly IDataConverter converter;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                /// <param name="converter">A DataConverter for simple conversions.</param>
                public Simple(HalSerializer parent, IDataConverter converter)
                    : base(parent)
                {
                    this.converter = converter;
                }

                public override T Deserialize(HalContext context, JToken token)
                    => Parent.converter.Convert((token as JValue)?.Value).To<T>();

                public override JToken Serialize(HalContext context, T item)
                    => Parent.converter.Convert(item).To<string>();
            }
            /// <summary>
            /// Typed HalSerializer for Nullable&lt;T&gt; types.
            /// </summary>
            public class Nullable : Typed<T>
            {
                private readonly Type valueType;
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public Nullable(HalSerializer parent)
                    : base(parent)
                {
                    valueType = typeof(T).GetGeneric(typeof(Nullable<>));
                    if (valueType == null)
                        throw new ArgumentException("Generic type should be Nullable<X>", nameof(T));
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }
                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);
                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);

                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(JToken), "input");

                    var block = Ex.Condition(
                            Ex.MakeBinary(System.Linq.Expressions.ExpressionType.Equal, Ex.Property(input, nameof(JToken.Type)), Ex.Constant(JTokenType.Null)),
                            Ex.Default(typeof(T)),
                            Ex.New(typeof(T).GetConstructor(new[] { valueType }),
                                Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { valueType }, ctx, input)));
                    return Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input).Compile();
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");

                    var block = Ex.Condition(
                        Ex.Property(input, nameof(Nullable<int>.HasValue)),
                        Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), new[] { valueType }, ctx, Ex.Property(input, nameof(Nullable<int>.Value))),
                        Ex.Constant(JValue.CreateNull(), typeof(JToken)));
                    return Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input).Compile();
                }
            }
            /// <summary>
            /// Default implementation for typed HalSerializers.
            /// Assumes a record like structure.
            /// Supports both mutable and immutable classes.
            /// </summary>
            public class Default : Typed<T>
            {
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public Default(HalSerializer parent)
                    : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);
                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);


                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var parameterlessConstructor = typeof(T).GetTypeInfo().GetConstructor(Type.EmptyTypes);
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(JToken), "input");

                    if (parameterlessConstructor != null)
                    {
                        var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                            .TakeWhile(x => x != null)
                            .SelectMany(x => x.DeclaredProperties)
                            .Where(p => p.CanRead && p.CanWrite && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                            .GroupBy(x => x.Name.ToUpperInvariant())
                            .Select(x => x.First())
                            .ToArray();
                        var result = Ex.Parameter(typeof(T), "result");

                        var block = Ex.Block(new[] { result },
                            Ex.Assign(result, Ex.New(parameterlessConstructor)),
                            Ex.Convert(Ex.Call(input, nameof(JToken.Children), new[] { typeof(JProperty) }), typeof(IEnumerable<JProperty>)).Foreach(current =>
                                Ex.Switch(Ex.Call(Ex.Property(current, nameof(JProperty.Name)), nameof(string.ToUpperInvariant), Type.EmptyTypes),

                                    props.Select(prop =>
                                        Ex.SwitchCase(
                                            Ex.Block(
                                                Ex.Assign(
                                                    Ex.Property(result, prop),
                                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { prop.PropertyType }, ctx, Ex.Property(current, nameof(JProperty.Value)))),
                                                Ex.Default(typeof(void))),
                                            Ex.Constant(prop.Name.ToUpperInvariant()))).ToArray())),
                            result);

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                    else
                    {
                        var ctor = typeof(T).GetConstructors().Where(c => c.IsPublic).OrderByDescending(c => c.GetParameters().Length).First();
                        var parameters = ctor.GetParameters().Select(p => Ex.Parameter(p.ParameterType, p.Name.ToUpperInvariant())).ToArray();
                        var block = Ex.Block(parameters,
                            Ex.Convert(Ex.Call(input, nameof(JToken.Children), new[] { typeof(JProperty) }), typeof(IEnumerable<JProperty>)).Foreach(current =>
                                Ex.Switch(
                                    Ex.Call(Ex.Property(current, nameof(JProperty.Name)), nameof(string.ToUpperInvariant), Type.EmptyTypes),
                                    parameters.Select(par =>
                                        Ex.SwitchCase(
                                            Ex.Block(
                                                Ex.Assign(par,
                                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { par.Type },
                                                        ctx, Ex.Property(current, nameof(JProperty.Value)))),
                                                Ex.Default(typeof(void))),
                                            Ex.Constant(par.Name.ToUpperInvariant())))
                                        .ToArray())),
                            Ex.New(ctor, parameters));

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");

                    var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                         .TakeWhile(x => x != null)
                         .SelectMany(x => x.DeclaredProperties)
                         .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                         .GroupBy(x => x.Name)
                         .Select(x => x.First())
                         .ToArray();
                    var serializeMethod = typeof(HalSerializer).GetMethods()
                        .Where(m => m.Name == nameof(HalSerializer.Serialize)
                            && m.ContainsGenericParameters
                            && m.GetGenericArguments().Length == 1
                            && m.GetParameters().Length == 2)
                        .First();

                    var block = Ex.New(typeof(JObject).GetConstructor(new[] { typeof(object[]) }),
                        Ex.NewArrayInit(typeof(object),
                            props.Select(prop =>
                                Ex.New(typeof(JProperty).GetConstructor(new[] { typeof(string), typeof(object) }),
                                    Ex.Constant(prop.Name.CasedToCamelCase()),
                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), Type.EmptyTypes, Ex.Constant(prop.PropertyType), ctx, Ex.Convert(Ex.Property(input, prop), typeof(object)))))));

                    var lambda = Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input);
                    return lambda.Compile();
                }

            }
            /// <summary>
            /// Typed HalSerializer for types that are represented through an ITypeRepresentation instance.
            /// </summary>
            public class Represented : Typed<T>
            {
                private readonly ITypeRepresentation representation;
                private readonly Type originalType;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                /// <param name="originalType">The original type that is passed for serialization.</param>
                /// <param name="representation">The type representation instance to use for serialization.</param>
                public Represented(HalSerializer parent, Type originalType, ITypeRepresentation representation) : base(parent)
                {
                    this.representation = representation;
                    this.originalType = originalType;
                }
                public override JToken Serialize(HalContext context, T item)
                {
                    var repr = representation.GetRepresentation(item);
                    var res = Parent.Serialize(context, repr);
                    return res;
                }
                public override T Deserialize(HalContext context, JToken token)
                {
                    var repr = Parent.Deserialize(representation.GetRepresentationType(typeof(T)), context, token);
                    return (T)representation.GetRepresentable(repr);
                }
            }
            /// <summary>
            /// Typed HalSerializer for collection types.
            /// </summary>
            public class Collection : Typed<T>
            {
                private readonly Type baseType;
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public Collection(HalSerializer parent) : base(parent)
                {
                    baseType = typeof(T).GetGeneric(typeof(IEnumerable<>));
                    if (baseType == null)
                        throw new ArgumentException("Generic type is not a collection");
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();

                }
                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);
                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);

                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var input = Ex.Parameter(typeof(JToken), "input");
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var children = Ex.Parameter(typeof(JToken[]), "children");
                    var idx = Ex.Parameter(typeof(int), "idx");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    if (typeof(T).IsArray || !typeof(ICollection<>).MakeGenericType(baseType).IsAssignableFrom(typeof(T)))
                    {
                        var result = Ex.Parameter(baseType.MakeArrayType(), "result");
                        var block = Ex.Block(new[] { children, idx, result },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(JToken)),
                                    Ex.Convert(
                                        Ex.Call(input, nameof(JToken.Children), Type.EmptyTypes),
                                        typeof(IEnumerable<JToken>)))),
                            Ex.Assign(result, Ex.NewArrayBounds(baseType, Ex.Property(children, nameof(Array.Length)))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Assign(Ex.ArrayAccess(result, idx),
                                Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { baseType },
                                    ctx, Ex.ArrayIndex(children, idx))),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            Ex.Convert(result, typeof(T)));

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                    else if (typeof(T).GetConstructor(Type.EmptyTypes) != null)
                    {
                        var result = Ex.Parameter(typeof(T), "result");
                        var block = Ex.Block(new[] { children, idx, result },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(JToken)),
                                    Ex.Convert(
                                        Ex.Call(input, nameof(JToken.Children), Type.EmptyTypes),
                                        typeof(IEnumerable<JToken>)))),
                            Ex.Assign(result, Ex.New(typeof(T).GetConstructor(Type.EmptyTypes))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Call(Ex.Convert(result, typeof(ICollection<>).MakeGenericType(baseType)), nameof(ICollection<object>.Add), Type.EmptyTypes,
                                Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { baseType },
                                    ctx, Ex.ArrayIndex(children, idx))),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            result);

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                    return null;
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var result = Ex.Parameter(typeof(List<object>), "result");
                    var block = Ex.Block(new[] { result },
                        Ex.Assign(result, Ex.New(typeof(List<object>))),
                        input.Foreach(item =>
                            Ex.Call(result, nameof(List<object>.Add), Type.EmptyTypes,
                                Ex.Convert(
                                    Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), Type.EmptyTypes, ctx, Ex.Convert(item, typeof(object))),
                                    typeof(object)))),
                        Ex.New(typeof(JArray).GetConstructor(new[] { typeof(object[]) }),
                            Ex.Call(typeof(Enumerable), nameof(Enumerable.ToArray), new[] { typeof(object) }, result)));

                    var lambda = Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input);
                    return lambda.Compile();
                }
            }
            /// <summary>
            /// Typed HalSerializer for Rest Values.
            /// </summary>
            public class RestValue : Typed<T>
            {
                private readonly Type valueType;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public RestValue(HalSerializer parent) : base(parent)
                {
                    if (!typeof(IRestValue).IsAssignableFrom(typeof(T)))
                        throw new ArgumentException("Type is not a RestValue");
                    valueType = typeof(T).GetGeneric(typeof(RestValue<>));
                }
                public override JToken Serialize(HalContext context, T item)
                {
                    var rv = (IRestValue)item;
                    context = context.With(rv);
                    var obj = Parent.Serialize(context, rv.Value);
                    var links = new JObject(from lnk in rv.Links
                                            group lnk by lnk.RelType into g
                                            select new JProperty(g.Key, g.Skip(1).Any()
                                                ? new JArray(g.Select(x => Parent.Serialize(context, x.Target)))
                                                : Parent.Serialize(context, g.First().Target)));
                    obj["_links"] = links;
                    obj["_embedded"] = new JArray(rv.Embeddings.Select(o => Parent.Serialize(o is IHasIdentity hid ? context.Without(hid.Id) : context, o)));
                    return obj;
                }
                public override T Deserialize(HalContext context, JToken token)
                {
                    throw new NotSupportedException();
                    // TODO: Missing deserialization type
                    //if (token is JObject o)
                    //{
                    //    var res = new EmptyRestValue();
                    //    var embedded = o["_embedded"];
                    //    if(embedded!=null && embedded is JArray es)
                    //    {

                    //        res.WithEmbeddings(es.Select(e => Parent.Deserialize()
                    //    }
                    //}
                    //else
                    //    throw new ArgumentException("Token should be object.");
                }
                private class EmptyRestValue : IRestValue
                {
                    public EmptyRestValue()
                        : this(Enumerable.Empty<Link>(), Enumerable.Empty<object>())
                    { }
                    public EmptyRestValue(IEnumerable<Link> links, IEnumerable<object> embeddings)
                    {
                        Links = links.ToArray();
                        Embeddings = embeddings.ToArray();
                    }
                    public object Value => null;

                    public Type ValueType => typeof(object);

                    public IReadOnlyList<Link> Links { get; }

                    public IReadOnlyList<object> Embeddings { get; }

                    public EmptyRestValue Manipulate(Func<IRestValue, IEnumerable<Link>> links = null, Func<IRestValue, IEnumerable<object>> embeddings = null)
                        => new EmptyRestValue(links(this), embeddings(this));
                    IRestValue IRestValue.Manipulate(Func<IRestValue, IEnumerable<Link>> links, Func<IRestValue, IEnumerable<object>> embeddings)
                        => Manipulate(links, embeddings);

                    public IRestValue AssignValue(Type valueType, object value)
                        => (IRestValue)Activator.CreateInstance(typeof(RestValue<>).MakeGenericType(valueType), value, Links, Embeddings);
                }
            }
            /// <summary>
            /// Typed HalSerializer for Dictionary-like types.
            /// </summary>
            public class Dictionary : Typed<T>
            {
                private readonly Type valueType;
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;
                public Dictionary(HalSerializer parent) : base(parent)
                {

                    var (keyType, valueType) = typeof(T).GetGenerics2(typeof(IDictionary<,>));
                    if (keyType == null || keyType != typeof(string) || valueType == null)
                        throw new ArgumentException("Generic type is not a proper dictionary");
                    this.valueType = valueType;
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }
                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);
                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);

                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(JToken), "input");
                    var result = Ex.Parameter(typeof(T), "result");
                    var props = Ex.Parameter(typeof(IEnumerable<JProperty>), "props");
                    if (valueType == typeof(object))
                    {
                        var block = Ex.Block(new[] { result, props },
                            Ex.Assign(result, Ex.New(typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType))),
                            Ex.Assign(props, Ex.Convert(Ex.Call(input, nameof(JToken.Children), new[] { typeof(JProperty) }), typeof(IEnumerable<JProperty>))),
                            props.Foreach(prop =>
                                Ex.Call(Ex.Convert(result, typeof(IDictionary<,>).MakeGenericType(typeof(string), valueType)), nameof(IDictionary<string, object>.Add), Type.EmptyTypes,
                                    Ex.Property(prop, nameof(JProperty.Name)),
                                    Ex.Condition(Ex.TypeIs(Ex.Property(prop, nameof(JProperty.Value)), typeof(JObject)),
                                        Ex.Convert(Ex.Call(Ex.Constant(this), nameof(Deserialize), Type.EmptyTypes,
                                            ctx, Ex.Property(prop, nameof(JProperty.First))), typeof(object)),
                                        Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), Type.EmptyTypes,
                                            Ex.Constant(typeof(string)), ctx, Ex.Property(prop, nameof(JProperty.First))), typeof(object))))),
                            result);

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                    else
                    {
                        var block = Ex.Block(new[] { result, props },
                            Ex.Assign(result, Ex.New(typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType))),
                            Ex.Assign(props, Ex.Convert(Ex.Call(input, nameof(JToken.Children), new[] { typeof(JProperty) }), typeof(IEnumerable<JProperty>))),
                            props.Foreach(prop =>
                                Ex.Call(Ex.Convert(result, typeof(IDictionary<,>).MakeGenericType(typeof(string), valueType)), nameof(IDictionary<string, object>.Add), Type.EmptyTypes,
                                    Ex.Property(prop, nameof(JProperty.Name)),
                                    Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { valueType },
                                        Ex.Constant(typeof(string)), ctx, Ex.Property(prop, nameof(JProperty.First))), typeof(object)))),
                            result);

                        var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, input);
                        return lambda.Compile();
                    }
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");
                    var kvp = Ex.Parameter(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), valueType), "kvp");
                    var select = typeof(Enumerable).GetMethods()
                        .Where(m => m.Name == nameof(Enumerable.Select)
                            && m.GetGenericArguments().Length == 2
                            && m.GetParameters().Length == 2
                            && m.GetParameters()[1].ParameterType == typeof(Func<,>).MakeGenericType(m.GetGenericArguments()[0], m.GetGenericArguments()[1]))
                        .First().MakeGenericMethod(kvp.Type, typeof(JProperty));
                    var ctor = typeof(JObject).GetConstructor(new[] { typeof(object) });
                    var pctor = typeof(JProperty).GetConstructor(new[] { typeof(string), typeof(object) });
                    var innerlambda = Ex.Lambda(
                        Ex.New(pctor,
                            Ex.Property(kvp, nameof(KeyValuePair<string, object>.Key)),
                            Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), Type.EmptyTypes,
                                ctx,
                                Ex.Convert(Ex.Property(kvp, nameof(KeyValuePair<string, object>.Value)), typeof(object)))),
                        kvp);

                    var block = Ex.Convert(Ex.New(ctor,
                        Ex.Convert(
                            Ex.Call(select,
                                Ex.Convert(input, typeof(IEnumerable<>).MakeGenericType(kvp.Type)),
                                innerlambda), typeof(object))), typeof(JToken));

                    var lambda = Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input);

                    return lambda.Compile();
                }
            }
            /// <summary>
            /// Typed HalSerializer for semantic structs.
            /// </summary>
            /// <typeparam name="P">The type of the underlying value.</typeparam>
            public class SemanticStruct<P> : Typed<T>
            {
                private readonly Func<HalContext, T, JToken> serializer;
                private readonly Func<HalContext, JToken, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                public SemanticStruct(HalSerializer parent) : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override JToken Serialize(HalContext context, T item)
                    => serializer(context, item);
                public override T Deserialize(HalContext context, JToken token)
                    => deserializer(context, token);
                private Func<HalContext, JToken, T> MakeDeserializer()
                {
                    var ctor = typeof(T).GetConstructor(new[] { typeof(P) });
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var token = Ex.Parameter(typeof(JToken), "token");
                    var block = Ex.New(ctor, Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Deserialize), new[] { typeof(P) },
                        ctx, token));
                    var lambda = Ex.Lambda<Func<HalContext, JToken, T>>(block, ctx, token);
                    return lambda.Compile();
                }

                private Func<HalContext, T, JToken> MakeSerializer()
                {
                    var prop = typeof(T).GetProperties().Where(p => p.PropertyType == typeof(P)).First();
                    var ctx = Ex.Parameter(typeof(HalContext), "ctx");
                    var input = Ex.Parameter(typeof(T), "input");
                    var block = Ex.Call(Ex.Constant(Parent), nameof(HalSerializer.Serialize), new[] { typeof(P) },
                        ctx,
                        Ex.Property(input, prop));
                    var lambda = Ex.Lambda<Func<HalContext, T, JToken>>(block, ctx, input);
                    return lambda.Compile();
                }
            }

            /// <summary>
            /// Typed HalSerializer for types that implement IHasIdentity.
            /// </summary>
            public class HasIdentity : Typed<T>
            {
                private readonly Typed<T> fallback;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                /// <param name="fallback">A fallback serializer, to serialize the type as if it didn't implement IHasIdentity.</param>
                public HasIdentity(HalSerializer parent, Typed<T> fallback)
                    : base(parent)
                {
                    if (!typeof(IHasIdentity).IsAssignableFrom(typeof(T)))
                        throw new ArgumentException("Type does not implement IHasIdentity.");
                    this.fallback = fallback;
                }

                public override T Deserialize(HalContext context, JToken token)
                {
                    if (token is JObject obj)
                    {
                        if (obj["id"] != null)
                        {
                            var id = Parent.Deserialize<IIdentity>(context, obj["id"]);
                            if (id != null && context.TryGetEmbedding(id, out var res) && res is T t)
                                return t;
                        }
                    }

                    return fallback.Deserialize(context, token);
                }

                public override JToken Serialize(HalContext context, T item)
                {
                    var id = ((IHasIdentity)item).Id;
                    if (context.TryGetEmbedding(id, out _))
                        return new JObject(new JProperty("id", Parent.Serialize(context, id)));
                    else
                        return fallback.Serialize(context, item);
                }
            }
        }
        /// <summary>
        /// Typed HalSerializer for DateTime.
        /// </summary>
        public class DateTime : Typed<System.DateTime>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parent">A reference to the parent HalSerializer instance.</param>
            public DateTime(HalSerializer parent) : base(parent)
            {
            }
            public override JToken Serialize(HalContext context, System.DateTime item)
            {
                return new JValue(Parent.converter.Convert(item).To<string>());
            }
            public override System.DateTime Deserialize(HalContext context, JToken token)
            {
                return Parent.converter.Convert((token as JValue)?.Value).To<System.DateTime>();
            }
        }
    }
}
