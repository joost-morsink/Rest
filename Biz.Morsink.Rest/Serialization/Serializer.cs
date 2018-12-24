using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    public partial class Serializer<C>
        where C : SerializationContext<C>
    {
        private readonly ConcurrentDictionary<Type, IForType> serializers;
        public ITypeDescriptorCreator TypeDescriptorCreator { get; }
        public IDataConverter Converter { get; }

        public Serializer(ITypeDescriptorCreator typeDescriptorCreator, IDataConverter converter = null)
        {
            serializers = new ConcurrentDictionary<Type, IForType>();
            TypeDescriptorCreator = typeDescriptorCreator;
            Converter = converter ?? DataConverter.Default;
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

            serializers[typeof(object)] = new Object(this);
        }

        private void AddSimple<T>()
        {
            serializers[typeof(T)] = new Typed<T>.Simple(this);
        }
        private IForType GetSerializer(Type t)
            => serializers.GetOrAdd(t, ty => CreateSerializer(ty));

        protected virtual IForType CreateSerializer(Type ty)
            => TypeDescriptorCreator.CreateSerializer(this, ty);

        private Typed<T> GetSerializer<T>()
            => (Typed<T>)GetSerializer(typeof(T));

        /// <summary>
        /// Serialize an object to an intermediate serialization format.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="context">The serialization context.</param>
        /// <param name="item">The object to serialize.</param>
        /// <returns>An intermediate representation of the object,</returns>
        public SItem Serialize<T>(C context, T item)
        {
            if (item == null)
                return SValue.Null;

            return GetSerializer<T>().Serialize(context, item);
        }
        /// <summary>
        /// Serialize an object to an intermediate serialization format.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="item">The object to serialize.</param>
        /// <returns>An intermediate representation of the object,</returns>
        public SItem Serialize(C context, object item)
        {
            if (item == null)
                return SValue.Null;
            var ty = item.GetType();

            return Serialize(context, ty, item);
        }
        /// <summary>
        /// Serialize an object to an intermediate serialization format.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="type">The type of the object.</param>15000
        /// <param name="item">The object to serialize.</param>
        /// <returns>An intermediate representation of the object,</returns>
        public SItem Serialize(C context, Type type, object item)
        {
            if (item == null)
                return SValue.Null;
            return GetSerializer(type).Serialize(context, item);
        }
        /// <summary>
        /// Deserialize an intermediate serialization object to a concrete type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="context">The serialization context.</param>
        /// <param name="item">The intermediate serialization object.</param>
        /// <returns>An instance of type T.</returns>
        public T Deserialize<T>(C context, SItem item)
        {
            if (item is SValue val && val.Value == null)
                return default;
            return GetSerializer<T>().Deserialize(context, item);
        }
        /// <summary>
        /// Deserialize an intermediate serialization object to a concrete type.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="type">The type to deserialize to.</param>
        /// <param name="item">The intermediate serialization object.</param>
        /// <returns>An instance of type T.</returns>
        public object Deserialize(C context, Type type, SItem item)
        {
            if (item is SValue val && val.Value == null)
                return default;
            return GetSerializer(type).Deserialize(context, item);
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
            /// <param name="context">The applicable serialization context.</param>
            /// <param name="item">An object of the correct type.</param>
            /// <returns>An SItem representing the serialized object.</returns>
            SItem Serialize(C context, object item);
            /// <summary>
            /// Deserializes an SItem to an object of the type.
            /// </summary>
            /// <param name="context">The applicable serialization context.</param>
            /// <param name="item">The SItem to deserialize.</param>
            /// <returns>A typed object constructed by the deserialization of the item.</returns>
            object Deserialize(C context, SItem item);
        }
    }
}
