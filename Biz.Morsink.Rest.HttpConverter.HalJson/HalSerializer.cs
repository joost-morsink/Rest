using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    /// <summary>
    /// Serializer class for Hal Json.
    /// </summary>
    public partial class HalSerializer
    {
        private readonly ConcurrentDictionary<Type, IForType> serializers;
        private readonly IDataConverter converter;
        private readonly ITypeRepresentation[] representations;
        private readonly IRestIdentityProvider identityProvider;
        private readonly TypeDescriptorCreator typeDescriptorCreator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDescriptorCreator">The type descriptor creator.</param>
        /// <param name="converter">A DataConverter instance for simple data conversions.</param>
        /// <param name="identityProvider">A Rest identity provider for Identity Link translations.</param>
        /// <param name="typeRepresentations">A collection of type representations.</param>
        public HalSerializer(TypeDescriptorCreator typeDescriptorCreator, IDataConverter converter, IRestIdentityProvider identityProvider, IEnumerable<ITypeRepresentation> typeRepresentations)
        {
            serializers = new ConcurrentDictionary<Type, IForType>();
            this.converter = converter;
            representations = typeRepresentations.ToArray();
            this.identityProvider = identityProvider;
            this.typeDescriptorCreator = typeDescriptorCreator;
            InitializeDefaultSerializers();
        }

        private void InitializeDefaultSerializers()
        {
            AddSimple<string>();
            AddSimple<bool>();
            AddSimple<DateTime>();

            AddSimple<long>();
            AddSimple<int>();
            AddSimple<short>();
            AddSimple<sbyte>();
            AddSimple<ulong>();
            AddSimple<uint>();
            AddSimple<ushort>();
            AddSimple<byte>();

            AddSimple<decimal>();
            AddSimple<float>();
            AddSimple<double>();
        }

        private void AddSimple<T>()
        {
            serializers[typeof(T)] = new Typed<T>.Simple(this, converter);
        }
        private HalContext EmptyContext()
            => HalContext.Create(identityProvider);
        /// <summary>
        /// Serializes an object to Hal Json with a new empty HalContext.
        /// </summary>
        /// <param name="item">The object to serialize.</param>
        /// <returns>A JToken representing the serialized object.</returns>
        public JToken Serialize(object item)
            => Serialize(EmptyContext(), item);
        /// <summary>
        /// Serializes an object to Hal Json with a new empty HalContext.
        /// </summary>
        /// <typeparam name="T">The type to find a specific serializer for.</typeparam>
        /// <param name="item">The object to serialize.</param>
        /// <returns>A JToken representing the serialized object.</returns>
        public JToken Serialize<T>(T item)
            => Serialize(EmptyContext(), item);
        /// <summary>
        /// Deserializes a piece of Json into an object of a specified type with a new empty HalContext.
        /// </summary>
        /// <param name="type">The type to use for deserialization.</param>
        /// <param name="token">The Json AST.</param>
        /// <returns>A deserialized object.</returns>
        public object Deserialize(Type type, JToken json)
            => Deserialize(type, EmptyContext(), json);
        /// <summary>
        /// Deserializes a piece of Json into an object of a specified type with a new empty HalContext.
        /// </summary>
        /// <typeparam name="T">The type to use for deserialization.</typeparam>
        /// <param name="token">The Json AST.</param>
        /// <returns>A deserialized object.</returns>
        public T Deserialize<T>(JToken json)
            => Deserialize<T>(EmptyContext(), json);
        /// <summary>
        /// Serializes an object to Hal Json.
        /// </summary>
        /// <param name="context">The applicable HalContext.</param>
        /// <param name="item">The object to serialize.</param>
        /// <returns>A JToken representing the serialized object.</returns>
        public JToken Serialize(HalContext context, object item)
        {
            if (item == null)
                return null;
            var serializer = GetSerializerForType(item.GetType());
            return serializer.Serialize(context, item);
        }
        /// <summary>
        /// Serializes an object to Hal Json.
        /// </summary>
        /// <param name="type">The type to find a specific serializer for.</param>
        /// <param name="context">The applicable HalContext.</param>
        /// <param name="item">The object to serialize.</param>
        /// <returns>A JToken representing the serialized object.</returns>
        public JToken Serialize(Type type, HalContext context, object item)
        {
            if (item == null)
                return null; 
            var serializer = GetSerializerForType(type);
            return serializer.Serialize(context, item);
        }
        /// <summary>
        /// Serializes an object to Hal Json.
        /// </summary>
        /// <typeparam name="T">The type to find a specific serializer for.</typeparam>
        /// <param name="context">The applicable HalContext.</param>
        /// <param name="item">The object to serialize.</param>
        /// <returns>A JToken representing the serialized object.</returns>
        public JToken Serialize<T>(HalContext context, T item)
        {
            if (item == null)
                return null;
            var serializer = GetSerializerForType<T>();
            return serializer.Serialize(context, item);
        } 
        /// <summary>
        /// Deserializes a piece of Json into an object of a specified type.
        /// </summary>
        /// <param name="type">The type to use for deserialization.</param>
        /// <param name="context">The applicable HalContext.</param>
        /// <param name="token">The Json AST.</param>
        /// <returns>A deserialized object.</returns>
        public object Deserialize(Type type, HalContext context, JToken token)
        {
            var serializer = GetSerializerForType(type);
            return serializer.Deserialize(context, token);
        }
        /// <summary>
        /// Deserializes a piece of Json into an object of a specified type.
        /// </summary>
        /// <typeparam name="T">The type to use for deserialization.</typeparam>
        /// <param name="context">The applicable HalContext.</param>
        /// <param name="token">The Json AST.</param>
        /// <returns>A deserialized object.</returns>
        public T Deserialize<T>(HalContext context, JToken token)
        {
            var serializer = GetSerializerForType<T>();
            return serializer.Deserialize(context, token);
        }
        /// <summary>
        /// Gets a specific serializer for a certain type.
        /// </summary>
        /// <param name="type">The serializable type.</param>
        /// <returns>A serializer capable of (de-)serializing objects of the specified type.</returns>
        public IForType GetSerializerForType(Type type)
            => serializers.GetOrAdd(type, Get);
        /// <summary>
        /// Gets a specific serializer for a certain type.
        /// </summary>
        /// <typeparam name="T">The serializable type.</typeparam>
        /// <returns>A serializer capable of (de-)serializing objects of the specified type.</returns>
        public Typed<T> GetSerializerForType<T>()
            => (Typed<T>)GetSerializerForType(typeof(T));
        private IForType Get(Type t)
        {
            if (typeof(IHasIdentity).IsAssignableFrom(t))
                return (IForType)Activator.CreateInstance(typeof(Typed<>.HasIdentity).MakeGenericType(t), this, inner());
            else
                return inner();

            IForType inner()
            {
                var repr = representations.FirstOrDefault(r => r.IsRepresentable(t));
                if (repr != null)
                    return (IForType)Activator.CreateInstance(typeof(Typed<>.Represented).MakeGenericType(t), this, t, repr);
                else if (typeof(IRestValue).IsAssignableFrom(t))
                    return (IForType)Activator.CreateInstance(typeof(Typed<>.RestValue).MakeGenericType(t), this);
                else if (t.GetGenerics2(typeof(IDictionary<,>)).Item1 == typeof(string))
                    return (IForType)Activator.CreateInstance(typeof(Typed<>.Dictionary).MakeGenericType(t), this);
                else if (typeof(IEnumerable).IsAssignableFrom(t))
                    return (IForType)Activator.CreateInstance(typeof(Typed<>.Collection).MakeGenericType(t), this);
                else if (t.GetGeneric(typeof(Nullable<>)) != null)
                    return (IForType)Activator.CreateInstance(typeof(Typed<>.Nullable).MakeGenericType(t), this);
                else if (SemanticStructKind.Instance.IsOfKind(t))
                    return (IForType)Activator.CreateInstance(typeof(Typed<>.SemanticStruct<>).MakeGenericType(t, SemanticStructKind.GetUnderlyingType(t)), this);
                else if (t == typeof(System.DateTime))
                    return new DateTime(this);
                else if (UnionRepresentationDescriptorKind.IsOfKind(t))
                    return (IForType)Activator.CreateInstance(typeof(Typed<>.UnionRep).MakeGenericType(t), this);
                else
                    return (IForType)Activator.CreateInstance(typeof(Typed<>.Default).MakeGenericType(t), this);
            }
        }
        /// <summary>
        /// Interface for a serializer for a specific type.
        /// </summary>
        public interface IForType
        {
            /// <summary>
            /// The type the implementation handles.
            /// </summary>
            Type Type { get; }
            /// <summary>
            /// Serializes an object of the type.
            /// </summary>
            /// <param name="context">The applicable HalContext.</param>
            /// <param name="item">An object of the correct type.</param>
            /// <returns>A JToken representing the serialized object.</returns>
            JToken Serialize(HalContext context, object item);
            /// <summary>
            /// Deserializes a JToken to an object of the type.
            /// </summary>
            /// <param name="context">The applicable HalContext.</param>
            /// <param name="token">The JToken to deserialize.</param>
            /// <returns>A typed object constructed by the deserialization of the token.</returns>
            object Deserialize(HalContext context, JToken token);
        }
    }
}
