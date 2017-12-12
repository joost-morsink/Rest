using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// This class provides XML serialization and deserialization.
    /// </summary>
    public class XmlSerializer
    {
        private static string StripName(string name)
        {
            if (name.Contains('`'))
                return name.Substring(0, name.IndexOf('`'));
            else
                return name;
        }
        private readonly ConcurrentDictionary<Type, IForType> serializers;
        private readonly TypeDescriptorCreator typeDescriptorCreator;
        private readonly IDataConverter converter;
        private readonly IReadOnlyList<ITypeRepresentation> representations;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDescriptorCreator">A TypeDescriptorCreator instance.</param>
        /// <param name="converter">An IDataConverter instance.</param>
        /// <param name="representations">A collection of ITypeRepresentation instances.</param>
        public XmlSerializer(TypeDescriptorCreator typeDescriptorCreator, IDataConverter converter, IEnumerable<ITypeRepresentation> representations)
        {
            serializers = new ConcurrentDictionary<Type, IForType>();
            this.typeDescriptorCreator = typeDescriptorCreator;
            this.converter = converter;
            this.representations = representations.ToArray();
            InitializeDefaultSerializers();
        }
        private void AddSimple<T>(ConcurrentDictionary<Type, IForType> dict)
            => dict[typeof(T)] = new Typed<T>.Simple(this, converter);
        private void InitializeDefaultSerializers()
        {
            AddSimple<string>(serializers);
            AddSimple<bool>(serializers);
            AddSimple<DateTime>(serializers);

            AddSimple<long>(serializers);
            AddSimple<int>(serializers);
            AddSimple<short>(serializers);
            AddSimple<sbyte>(serializers);
            AddSimple<ulong>(serializers);
            AddSimple<uint>(serializers);
            AddSimple<ushort>(serializers);
            AddSimple<byte>(serializers);

            AddSimple<decimal>(serializers);
            AddSimple<float>(serializers);
            AddSimple<double>(serializers);
        }
        /// <summary>
        /// Serializes an object into an XElement.
        /// </summary>
        /// <param name="item">The object to serialize. The method reflects on the type of the object.</param>
        /// <returns>An XElement representing the serialized item.</returns>
        public XElement Serialize(object item)
        {
            if (item == null)
                return null;
            var serializer = GetSerializerForType(item.GetType());
            return serializer.Serialize(item);
        }
        /// <summary>
        /// Serializes an object into an XElement.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="item">The object to serialize.</param>
        /// <returns>An XElement representing the serialized item.</returns>
        public XElement Serialize<T>(T item)
        {
            if (item == null)
                return null;
            var serializer = GetSerializerForType<T>();
            return serializer.Serialize(item);
        }
        /// <summary>
        /// Deserializes an XElement to an object of the specified type.
        /// </summary>
        /// <param name="element">The element to deserialize.</param>
        /// <param name="type">The type of object to construct.</param>
        /// <returns>An object constructed by deserialization of the element.</returns>
        public object Deserialize(XElement element, Type type)
        {
            var serializer = GetSerializerForType(type);
            return serializer.Deserialize(element);
        }        /
        /// <summary>
        /// Deserializes an XElement to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to construct.</typeparam>
        /// <param name="element">The element to deserialize.</param>
        /// <returns>An object constructed by deserialization of the element.</returns>
        public T Deserialize<T>(XElement element)
        {
            var serializer = GetSerializerForType<T>();
            return serializer.Deserialize(element);
        }

        /// <summary>
        /// Gets a serializer for a specific type.
        /// </summary>
        /// <param name="type">The type the serializer should handle.</param>
        /// <returns>An XmlSerializer.IForType instance.</returns>
        public IForType GetSerializerForType(Type type)
            => serializers.GetOrAdd(type, get);
        /// <summary>
        /// Gets a typed serializer for a specific type.
        /// </summary>
        /// <typeparam name="T">The type the serializer should handle.</typeparam>
        /// <returns>An XmlSerializer.Typed&lt;T&gt; instance.</returns>
        public Typed<T> GetSerializerForType<T>()
            => (Typed<T>)GetSerializerForType(typeof(T));

        private IForType get(Type t)
        {
            var repr = representations.FirstOrDefault(r => r.IsRepresentable(t));

            var res = (IForType)(repr == null
                ? t.GetGenerics2(typeof(IDictionary<,>)).Item1 == typeof(string)
                    ? Activator.CreateInstance(typeof(Typed<>.Dictionary).MakeGenericType(t), this)
                    : typeof(IEnumerable).IsAssignableFrom(t)
                        ? Activator.CreateInstance(typeof(Typed<>.Collection).MakeGenericType(t), this)
                        : Activator.CreateInstance(typeof(Typed<>.Default).MakeGenericType(t), this)
                : Activator.CreateInstance(typeof(Typed<>.Represented).MakeGenericType(t), this, t, repr));

            return res;
        }

        #region Helper types
        /// <summary>
        /// Interface for a serializer for a specific type.
        /// </summary>
        public interface IForType
        {
            /// <summary>
            /// The type the implementation handles.
            /// </summary>
            Type ForType { get; }
            /// <summary>
            /// Serializes an object of the type.
            /// </summary>
            /// <param name="item">An object of the correct type.</param>
            /// <returns>The XElement representing the serialized object.</returns>
            XElement Serialize(object item);
            /// <summary>
            /// Deserializes an XElement to an object of the type.
            /// </summary>
            /// <param name="element">The XElement to deserialize.</param>
            /// <returns>A typed object constructed by the deserialization of the element.</returns>
            object Deserialize(XElement element);
        }
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
            /// Should implement deserialization to object of type T.
            /// </summary>
            /// <param name="e">The XElement to deserialize.</param>
            /// <returns>A deserialized object of type T.</returns>
            public abstract T Deserialize(XElement e);

            Type IForType.ForType => typeof(T);
            XElement IForType.Serialize(object item) => Serialize((T)item);
            object IForType.Deserialize(XElement element) => Deserialize(element);
            [Obsolete]
            public class DotNetDefault : Typed<T>
            {
                private System.Xml.Serialization.XmlSerializer serializer;
                public DotNetDefault(XmlSerializer parent) : base(parent)
                {
                    serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                }

                public override XElement Serialize(T item)
                {
                    var root = new XElement("root");
                    using (var wri = root.CreateWriter())
                        serializer.Serialize(wri, item);
                    return root.Elements().First();
                }
                public override T Deserialize(XElement e)
                {
                    using (var rdr = e.CreateReader())
                        return (T)serializer.Deserialize(rdr);
                }
            }
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

                public override T Deserialize(XElement e) => converter.Convert(e.Value).TryTo(out T res) ? res : default(T);
                public override XElement Serialize(T item) => new XElement("simple", converter.Convert(item).To<string>());

            }
            /// <summary>
            /// Typed XmlSerializer for Dictionary-like types.
            /// </summary>
            public class Dictionary : Typed<T>
            {
                private readonly Type valueType;
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
                    return null;
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
                    var enumerator = Ex.Parameter(typeof(IEnumerator<>).MakeGenericType(basetype), "enumerator");
                    var item = Ex.Parameter(basetype, "item");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    var block = Ex.Block(new[] { item, enumerator, result },
                        Ex.Assign(result, Ex.New(typeof(XElement).GetConstructor(new[] { typeof(XName) }), Ex.Constant((XName)"Array"))),
                        Ex.Assign(enumerator, Ex.Call(input, typeof(IEnumerable<>).MakeGenericType(basetype).GetMethod(nameof(IEnumerable<object>.GetEnumerator)))),
                        Ex.Label(start),
                        Ex.IfThen(Ex.Not(Ex.Call(Ex.Convert(enumerator, typeof(IEnumerator)), nameof(IEnumerator.MoveNext), Type.EmptyTypes)),
                            Ex.Goto(end, result)),
                        Ex.Assign(item, Ex.Property(enumerator, nameof(IEnumerator<object>.Current))),
                        Ex.Call(result, nameof(XElement.Add), Type.EmptyTypes,
                            Ex.Convert(Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                Ex.Convert(item, typeof(object))),
                                typeof(object))),
                        Ex.Goto(start),
                        Ex.Label(end),
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
                private Func<T, XElement> serializer;
                private Func<XElement, T> deserializer;
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
                                Ex.Call(typeof(Utils), nameof(Utils.GetContent), Type.EmptyTypes,
                                    Ex.Call(Ex.Constant(Parent), nameof(XmlSerializer.Serialize), Type.EmptyTypes,
                                        Ex.Convert(Ex.Property(input, prop), typeof(object))))))));
                    var lambda = Ex.Lambda(block, input);
                    return (Func<T, XElement>)lambda.Compile();
                }
                private Func<XElement, T> MakeDeserializer()
                {
                    var parameterlessConstructor = typeof(T).GetTypeInfo().GetConstructor(Type.EmptyTypes);
                    var input = Ex.Parameter(typeof(XElement), "input");
                    var enumerator = Ex.Parameter(typeof(IEnumerator<XElement>), "enumerator");
                    var start = Ex.Label("start");
                    var end = Ex.Label("end");
                    var current = Ex.Parameter(typeof(XElement), "current");

                    if (parameterlessConstructor != null)
                    {
                        var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo())
                            .TakeWhile(x => x != null)
                            .SelectMany(x => x.DeclaredProperties)
                            .Where(p => p.CanRead && p.CanWrite && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                            .GroupBy(x => x.Name.ToUpperInvariant())
                            .Select(x => x.First())
                            .ToArray();
                        var result = Ex.Parameter(typeof(T), "res");
                        var block = Ex.Block(new[] { result, enumerator, current },
                            Ex.Assign(result, Ex.New(parameterlessConstructor)),
                            Ex.Assign(enumerator,
                                Ex.Call(Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes),
                                    nameof(IEnumerable<object>.GetEnumerator), Type.EmptyTypes)),
                            Ex.Label(start),

                            Ex.IfThen(Ex.Not(Ex.Call(Ex.Convert(enumerator, typeof(IEnumerator)),
                                    nameof(IEnumerator.MoveNext), Type.EmptyTypes)),
                                Ex.Goto(end)),
                            Ex.Assign(current, Ex.Property(enumerator, nameof(IEnumerator<XElement>.Current))),
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
                                        Ex.Constant(prop.Name.ToUpperInvariant()))).ToArray()),
                            Ex.Goto(start),
                            Ex.Label(end),
                            result);

                        var lambda = Ex.Lambda<Func<XElement, T>>(block, input);
                        return lambda.Compile();
                    }
                    else
                    {
                        var ctor = typeof(T).GetConstructors().Where(c => c.IsPublic).OrderByDescending(c => c.GetParameters().Length).First();
                        var parameters = ctor.GetParameters().Select(p => Ex.Parameter(p.ParameterType, p.Name.ToUpperInvariant())).ToArray();
                        var block = Ex.Block(new[] { enumerator, current }.Concat(parameters),
                            Ex.Assign(enumerator,
                                Ex.Call(Ex.Call(input, nameof(XElement.Elements), Type.EmptyTypes),
                                    nameof(IEnumerable<object>.GetEnumerator), Type.EmptyTypes)),
                            Ex.Label(start),

                            Ex.IfThen(Ex.Not(Ex.Call(Ex.Convert(enumerator, typeof(IEnumerator)),
                                    nameof(IEnumerator.MoveNext), Type.EmptyTypes)),
                                Ex.Goto(end)),
                            Ex.Assign(current, Ex.Property(enumerator, nameof(IEnumerator<XElement>.Current))),
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
                                        Ex.Constant(par.Name.ToUpperInvariant()))).ToArray()),
                            Ex.Goto(start),
                            Ex.Label(end),
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
        }
        #endregion
    }
}
