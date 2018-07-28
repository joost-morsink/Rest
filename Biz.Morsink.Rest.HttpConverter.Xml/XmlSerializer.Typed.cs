using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.Rest.HttpConverter.Xml.XsdConstants;
using Biz.Morsink.Rest.Utils;
using System.Collections.Immutable;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public partial class XmlSerializer
    {
        /// <summary>
        /// Abstract base class for serializers that handle a specific single type.
        /// </summary>
        /// <typeparam name="T">The type the serializer handles.</typeparam>
        public abstract class Typed<T> : IForType
        {
            /// <summary>
            /// Gets a reference to the parent XmlSerializer instance.
            /// </summary>
            protected XmlSerializer Parent { get; }
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
            public Typed(XmlSerializer parent)
            {
                Parent = parent;
            }
            /// <summary>
            /// Should implement serialization for objects of type T.
            /// </summary>
            /// <param name="item">The object to serialize.</param>
            /// <returns>The serialization of the object as an XElement.</returns>
            public abstract XElement Serialize(T item);
            /// <summary>
            /// Should implement deserialization to objects of type T.
            /// </summary>
            /// <param name="e">The XElement to deserialize.</param>
            /// <returns>A deserialized object of type T.</returns>
            public abstract T Deserialize(XElement e);

            Type IForType.ForType => typeof(T);
            XElement IForType.Serialize(object item) => Serialize((T)item);
            object IForType.Deserialize(XElement element) => Deserialize(element);

            /// <summary>
            /// Typed XmlSerializer for simple (primitive) 'tostring' types.
            /// </summary>
            public class Simple : Typed<T>
            {
                private readonly IDataConverter converter;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                /// <param name="converter">An IDataConverter instance for converting to and from string.</param>
                public Simple(XmlSerializer parent, IDataConverter converter) : base(parent)
                {
                    this.converter = converter;
                }

                public override T Deserialize(XElement e) => converter.Convert(e.Value).TryTo(out T res) ? res : default;
                public override XElement Serialize(T item)
                {
                    var ty = item.GetType();
                    if (converter.Convert(item).TryTo(out string str) && str != null)
                        return new XElement("simple", converter.Convert(item).To<string>());
                    else
                        return new XElement("simple", new XAttribute(XSI + nil, true));
                }

            }
            /// <summary>
            /// Typed XmlSerializer for Nullable&lt;T&gt; types.
            /// </summary>
            public class Nullable : Typed<T>
            {
                private readonly Type valueType;
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent"></param>
                public Nullable(XmlSerializer parent) : base(parent)
                {
                    valueType = typeof(T).GetGeneric(typeof(Nullable<>));
                    if (valueType == null)
                        throw new ArgumentException("Generic type should be Nullable<X>", nameof(T));
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);

                private Func<XElement, T> MakeDeserializer()
                {
                    var input = Ex.Parameter(typeof(XElement), "input");

                    var block = Ex.Condition(
                            Ex.MakeBinary(System.Linq.Expressions.ExpressionType.Equal, Ex.Property(input, nameof(XElement.Value)), Ex.Constant("")),
                            Ex.Default(typeof(T)),
                            Ex.New(typeof(T).GetConstructor(new[] { valueType }),
                                Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), new[] { valueType }, input)));
                    return Ex.Lambda<Func<XElement, T>>(block, input).Compile();
                }

                private Func<T, XElement> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var block = Ex.Condition(
                        Ex.Property(input, nameof(Nullable<int>.HasValue)),
                        Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), new[] { valueType }, Ex.Property(input, nameof(Nullable<int>.Value))),
                        Ex.New(typeof(XElement).GetConstructor(new Type[] { typeof(XName) }), Ex.Constant((XName)"nullable")));
                    return Ex.Lambda<Func<T, XElement>>(block, input).Compile();
                }
            }
            /// <summary>
            /// Typed XmlSerializer for Dictionary-like types.
            /// </summary>
            public class Dictionary : Typed<T>
            {
                private readonly Type valueType;
                private readonly Type kind;
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                public Dictionary(XmlSerializer parent) : base(parent)
                {
                    var (keyType, valueType) = typeof(T).GetGenerics2(typeof(IDictionary<,>));
                    if (keyType == null || keyType != typeof(string) || valueType == null)
                        throw new ArgumentException("Generic type is not a proper dictionary");
                    kind = typeof(T).IsGenericType ? typeof(T).GetGenericTypeDefinition() : typeof(T);
                    this.valueType = valueType;
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);

                private Func<XElement, T> MakeDeserializer()
                {
                    var input = Ex.Parameter(typeof(XElement), "input");
                    var @class = kind == typeof(IDictionary<,>) || kind == typeof(IImmutableDictionary<,>) || kind == typeof(ImmutableDictionary<,>)
                        ? typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType)
                        : typeof(T);
                    var result = Ex.Parameter(@class, "result");
                    var elements = Ex.Parameter(typeof(IEnumerable<XElement>), "elements");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    if (valueType == typeof(object))
                    {
                        var block = Ex.Block(new[] { result, elements },
                            Ex.Assign(result, Ex.New(@class)),
                            Ex.Assign(elements, Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes)),
                            elements.Foreach(current =>
                                Ex.Call(result, nameof(Dictionary<string, object>.Add), Type.EmptyTypes,
                                    Ex.Property(Ex.Property(current, nameof(XElement.Name)), nameof(XName.LocalName)),
                                    Ex.Condition(Ex.Property(current, nameof(XElement.HasElements)),
                                        Ex.Convert(Ex.Call(Ex.Constant(this), nameof(Deserialize), Type.EmptyTypes, current), typeof(object)),
                                        Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                            current, Ex.Constant(typeof(string))),
                                            typeof(object))))),
                            Ex.Convert(
                                kind == typeof(IImmutableDictionary<,>) || kind == typeof(ImmutableDictionary<,>)
                                    ? (Ex)Ex.Call(typeof(ImmutableDictionary), nameof(ImmutableDictionary.ToImmutableDictionary), new[] { typeof(string), valueType }, result)
                                    : result,
                                typeof(T)));
                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    else
                    {
                        var block = Ex.Block(new[] { result, elements },
                            Ex.Assign(result, Ex.New(@class)),
                            Ex.Assign(elements, Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes)),
                            elements.Foreach(current =>
                                Ex.Call(result, nameof(Dictionary<string, object>.Add), Type.EmptyTypes,
                                    Ex.Property(Ex.Property(current, nameof(XElement.Name)), nameof(XName.LocalName)),
                                    Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), new[] { valueType },
                                        current))),
                            Ex.Convert(
                                kind == typeof(IImmutableDictionary<,>) || kind == typeof(ImmutableDictionary<,>)
                                    ? (Ex)Ex.Call(typeof(ImmutableDictionary), nameof(ImmutableDictionary.ToImmutableDictionary), new[] { typeof(string), valueType }, result)
                                    : result,
                                typeof(T)));

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                }

                private Func<T, XElement> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var kvp = Ex.Parameter(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), valueType), "kvp");
                    var enumerator = Ex.Parameter(typeof(IEnumerator<>).MakeGenericType(kvp.Type), "enumerator");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    var ctor = typeof(XElement).GetConstructor(new[] { typeof(XName), typeof(object) });
                    var innerlambda = Ex.Lambda(
                        Ex.New(ctor,
                            Ex.Convert(Ex.Property(kvp, nameof(KeyValuePair<string, object>.Key)), typeof(XName)),
                            Ex.Call(typeof(Utils).GetMethod(nameof(Utils.GetContent)),
                                Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                    Ex.Convert(Ex.Property(kvp, nameof(KeyValuePair<string, object>.Value)), typeof(object))))), kvp);

                    var block = Ex.New(ctor,
                        Ex.Convert(Ex.Constant(StripName(typeof(T).Name)), typeof(XName)),
                        Ex.Convert(
                            Ex.Call(typeof(Enumerable).GetMethods()
                                .Where(m => m.Name == nameof(Enumerable.Select)
                                    && m.GetGenericArguments().Length == 2
                                    && m.GetParameters().Length == 2
                                    && m.GetParameters()[1].ParameterType == typeof(Func<,>).MakeGenericType(m.GetGenericArguments()[0], m.GetGenericArguments()[1]))
                                .First().MakeGenericMethod(kvp.Type, typeof(XElement)),
                                Ex.Convert(input, typeof(IEnumerable<>).MakeGenericType(kvp.Type)),
                                innerlambda), typeof(object)));
                    var lambda = Ex.Lambda<Func<T, XElement>>(block, input);

                    return lambda.Compile();
                }
            }
            /// <summary>
            /// Typed XmlSerializer for collection types.
            /// </summary>
            public class Collection : Typed<T>
            {
                private readonly Type basetype;
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                public Collection(XmlSerializer parent) : base(parent)
                {
                    basetype = typeof(T).GetGeneric(typeof(IEnumerable<>));
                    if (basetype == null)
                        throw new ArgumentException("Generic type is not a collection");
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }
                private Func<T, XElement> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var result = Ex.Parameter(typeof(XElement), "result");
                    var block = Ex.Block(new[] { result },
                        Ex.Assign(result, Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName) }), Ex.Constant((XName)"Array"))),
                        input.Foreach(item =>
                            Ex.Call(result, nameof(XElement.Add), Type.EmptyTypes,
                                Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                    Ex.Convert(item, typeof(object))),
                                    typeof(object)))),
                        result);
                    var lambda = Ex.Lambda(block, input);
                    return (Func<T, XElement>)lambda.Compile();
                }
                private Func<XElement, T> MakeDeserializer()
                {
                    var input = Ex.Parameter(typeof(XElement), "input");
                    var children = Ex.Parameter(typeof(XElement[]), "children");
                    var idx = Ex.Parameter(typeof(int), "idx");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    if (typeof(T).IsArray || !typeof(ICollection<>).MakeGenericType(basetype).IsAssignableFrom(typeof(T)))
                    {
                        var result = Ex.Parameter(basetype.MakeArrayType(), "tmp");
                        var block = Ex.Block(new[] { children, idx, result },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(XElement)),
                                    Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes))),
                            Ex.Assign(result, Ex.NewArrayBounds(basetype, Ex.Property(children, nameof(Array.Length)))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Assign(Ex.ArrayAccess(result, idx),
                                Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                    Ex.ArrayIndex(children, idx), Ex.Constant(basetype)), basetype)),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            Ex.Convert(result, typeof(T)));

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    else if (typeof(T).GetConstructor(new [] { typeof(IEnumerable<>).MakeGenericType(basetype) })!=null)
                    {
                        var elements = Ex.Parameter(typeof(List<>).MakeGenericType(basetype), "elements");
                        var block = Ex.Block(new[] { children, idx, elements },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(XElement)),
                                    Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Assign(elements, Ex.New(elements.Type)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Call(elements, nameof(List<object>.Add), Type.EmptyTypes,
                                Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                    Ex.ArrayIndex(children, idx), Ex.Constant(basetype)), basetype)),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            Ex.New(typeof(T).GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(basetype) }),
                                Ex.Convert(elements, typeof(IEnumerable<>).MakeGenericType(basetype))));

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    else if (typeof(T).GetConstructor(Type.EmptyTypes) != null)
                    {
                        var result = Ex.Parameter(typeof(T), "result");
                        var block = Ex.Block(new[] { children, idx, result },
                            Ex.Assign(children,
                                Ex.Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(XElement)),
                                    Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes))),
                            Ex.Assign(result, Ex.New(typeof(T).GetConstructor(Type.EmptyTypes))),
                            Ex.Assign(idx, Ex.Constant(0)),
                            Ex.Label(start),
                            Ex.IfThen(Ex.MakeBinary(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual, idx, Ex.Property(children, nameof(Array.Length))),
                                Ex.Goto(end)),
                            Ex.Call(Ex.Convert(result, typeof(ICollection<>).MakeGenericType(basetype)), nameof(ICollection<object>.Add), Type.EmptyTypes,
                                Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                    Ex.ArrayIndex(children, idx), Ex.Constant(basetype)), basetype)),
                            Ex.Assign(idx, Ex.Increment(idx)),
                            Ex.Goto(start),
                            Ex.Label(end),
                            result);

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    return null;
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);
            }
            /// <summary>
            /// Default implementation for typed XmlSerializers.
            /// Assumes a 'record-like' structure.
            /// Supports both mutable and immutable classes.
            /// </summary>
            public class Default : Typed<T>
            {
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                public Default(XmlSerializer parent) : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);

                private Func<T, XElement> MakeSerializer()
                {
                    var input = Ex.Parameter(typeof(T), "input");
                    var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                        .TakeWhile(x => x != null)
                        .SelectMany(x => x.DeclaredProperties)
                        .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                        .GroupBy(x => x.Name)
                        .Select(x => x.First())
                        .ToArray();
                    var block = Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName), typeof(object[]) }),
                        Ex.Convert(Ex.Constant(StripName(typeof(T).Name)), typeof(XName)),
                        Ex.NewArrayInit(typeof(object),
                            props.Select(prop =>
                                Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName), typeof(object) }),
                                Ex.Convert(Ex.Constant(prop.Name), typeof(XName)),
                                Ex.Call(typeof(Utils), nameof(Utils.GetContentOrNil), Type.EmptyTypes,
                                    Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                        Ex.Constant(prop.PropertyType),
                                        Ex.Convert(Ex.Property(input, prop), typeof(object))))))));
                    var lambda = Ex.Lambda(block, input);
                    return (Func<T, XElement>)lambda.Compile();
                }
                private Func<XElement, T> MakeDeserializer()
                {
                    var parameterlessConstructor = typeof(T).GetTypeInfo().GetConstructor(Type.EmptyTypes);
                    var input = Ex.Parameter(typeof(XElement), "input");


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
                            Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes).Foreach(current =>
                                Ex.Switch(
                                    Ex.Call(Ex.Property(Ex.Property(current, nameof(XElement.Name)), nameof(XName.LocalName)), nameof(string.ToUpperInvariant), Type.EmptyTypes),
                                    props.Select(prop =>
                                        Ex.SwitchCase(
                                            Ex.Block(
                                                Ex.Assign(
                                                    Ex.Property(result, prop),
                                                    Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                                        current, Ex.Constant(prop.PropertyType)), prop.PropertyType)),
                                                Ex.Default(typeof(void))),
                                            Ex.Constant(prop.Name.ToUpperInvariant()))).ToArray())),
                            result);

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    else
                    {
                        var ctor = typeof(T).GetConstructors().Where(c => c.IsPublic).OrderByDescending(c => c.GetParameters().Length).First();
                        var parameters = ctor.GetParameters().Select(p => Ex.Parameter(p.ParameterType, p.Name.ToUpperInvariant())).ToArray();
                        var block = Ex.Block(parameters,
                            Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes).Foreach(current =>
                                Ex.Block(
                                    Ex.Switch(
                                    Ex.Call(Ex.Property(Ex.Property(current, nameof(XElement.Name)), nameof(XName.LocalName)), nameof(string.ToUpperInvariant), Type.EmptyTypes),
                                    parameters.Select(par =>
                                        Ex.SwitchCase(
                                            Ex.Block(
                                                Ex.Assign(
                                                    par,
                                                    Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), Type.EmptyTypes,
                                                        current, Ex.Constant(par.Type)), par.Type)),
                                                Ex.Default(typeof(void))),
                                            Ex.Constant(par.Name.ToUpperInvariant()))).ToArray()))),
                            Ex.New(ctor, parameters.ToArray()));

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                }
            }
            /// <summary>
            /// Typed XmlSerializer for types that are represented through an ITypeRepresentation instance.
            /// </summary>
            public class Represented : Typed<T>
            {
                private readonly ITypeRepresentation representation;
                private readonly Type originalType;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                /// <param name="originalType">Equals typeof(T).</param>
                /// <param name="representation">The type representation instance to use for transformations.</param>
                public Represented(XmlSerializer parent, Type originalType, ITypeRepresentation representation) : base(parent)
                {
                    this.representation = representation;
                    this.originalType = originalType;
                }
                public override XElement Serialize(T item)
                {
                    var repr = representation.GetRepresentation(item);
                    var res = Parent.Serialize(repr);
                    return new XElement(StripName(originalType.Name), res.GetContent());
                }
                public override T Deserialize(XElement e)
                {
                    var repr = Parent.Deserialize(e, representation.GetRepresentationType(typeof(T)));
                    return (T)representation.GetRepresentable(repr);
                }
            }
            /// <summary>
            /// Typed XmlSerializer for which the operations are delegated to functions.
            /// </summary>
            public class Delegated : Typed<T>
            {
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">The parent serializer.</param>
                /// <param name="serializer">A function used for serialization.</param>
                /// <param name="deserializer">A function used for deserialization.</param>
                public Delegated(XmlSerializer parent, Func<T, XElement> serializer, Func<XElement, T> deserializer) : base(parent)
                {
                    this.serializer = serializer;
                    this.deserializer = deserializer;
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);
            }
            /// <summary>
            /// Typed XmlSerializer for semantic structs.
            /// </summary>
            /// <typeparam name="P">The type of the underlying value.</typeparam>
            public class SemanticStruct<P> : Typed<T>
            {
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">The parent serializer.</param>
                public SemanticStruct(XmlSerializer parent) : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }

                private Func<XElement, T> MakeDeserializer()
                {
                    var ctor = typeof(T).GetConstructor(new[] { typeof(P) });
                    var e = Ex.Parameter(typeof(XElement), "e");
                    var block = Ex.New(ctor, Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Deserialize), new[] { typeof(P) }, e));
                    var lambda = Ex.Lambda<Func<XElement, T>>(block, e);
                    return lambda.Compile();
                }

                private Func<T, XElement> MakeSerializer()
                {
                    var prop = typeof(T).GetProperties().Where(p => p.PropertyType == typeof(P)).First();
                    var t = Ex.Parameter(typeof(T), "t");
                    var block = Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), new[] { typeof(P) }, Ex.Property(t, prop));
                    var lambda = Ex.Lambda<Func<T, XElement>>(block, t);
                    return lambda.Compile();
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);
                public override XElement Serialize(T item)
                    => serializer(item);
            }
        }

    }
}
