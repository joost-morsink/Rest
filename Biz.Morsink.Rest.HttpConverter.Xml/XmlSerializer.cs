using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
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
    public partial class XmlSerializer
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
        private readonly ITypeRepresentation[] representations;
        private readonly IXmlSchemaTranslator[] schemaTranslators;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDescriptorCreator">A TypeDescriptorCreator instance.</param>
        /// <param name="converter">An IDataConverter instance.</param>
        /// <param name="representations">A collection of ITypeRepresentation instances.</param>
        public XmlSerializer(TypeDescriptorCreator typeDescriptorCreator, IDataConverter converter, IEnumerable<IXmlSchemaTranslator> schemaTranslators, IEnumerable<ITypeRepresentation> representations)
        {
            serializers = new ConcurrentDictionary<Type, IForType>();
            this.typeDescriptorCreator = typeDescriptorCreator;
            this.converter = converter;
            this.representations = representations.ToArray();
            this.schemaTranslators = schemaTranslators.ToArray();
            InitializeDefaultSerializers();
            foreach (var schemaTranslator in schemaTranslators)
                schemaTranslator.SetSerializer(this);
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
        /// <param name="item">The object to serialize.</param>
        /// <param name="type">The type used to serialize.</param>
        /// <returns>An XElement representing the serialized item.</returns>
        public XElement Serialize(Type type, object item)
        {
            if (item == null)
                return null;
            var serializer = GetSerializerForType(type);
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
        }
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
            => serializers.GetOrAdd(type, Get);
        /// <summary>
        /// Gets a typed serializer for a specific type.
        /// </summary>
        /// <typeparam name="T">The type the serializer should handle.</typeparam>
        /// <returns>An XmlSerializer.Typed&lt;T&gt; instance.</returns>
        public Typed<T> GetSerializerForType<T>()
            => (Typed<T>)GetSerializerForType(typeof(T));

        private IForType Get(Type t)
        {
            var trans = schemaTranslators.FirstOrDefault(st => st.ForType.IsAssignableFrom(t));
            if (trans != null)
                return trans.GetConverter();
            var repr = representations.FirstOrDefault(r => r.IsRepresentable(t));
            if (repr != null)
                return (IForType)Activator.CreateInstance(typeof(Typed<>.Represented).MakeGenericType(t), this, t, repr);
            else if (t.GetGenerics2(typeof(IDictionary<,>)).Item1 == typeof(string))
                return (IForType)Activator.CreateInstance(typeof(Typed<>.Dictionary).MakeGenericType(t), this);
            else if (typeof(IEnumerable).IsAssignableFrom(t))
                return (IForType)Activator.CreateInstance(typeof(Typed<>.Collection).MakeGenericType(t), this);
            else if (t.GetGeneric(typeof(Nullable<>)) != null)
                return (IForType)Activator.CreateInstance(typeof(Typed<>.Nullable).MakeGenericType(t), this);
            else if (SemanticStructKind.Instance.IsOfKind(t))
                return (IForType)Activator.CreateInstance(typeof(Typed<>.SemanticStruct<>).MakeGenericType(t, SemanticStructKind.GetUnderlyingType(t)), this);
            else
                return (IForType)Activator.CreateInstance(typeof(Typed<>.Default).MakeGenericType(t), this);
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
        #endregion
    }
}
