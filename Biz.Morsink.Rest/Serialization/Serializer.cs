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
        public TypeDescriptorCreator TypeDescriptorCreator { get; }
        private readonly ITypeRepresentation[] typeRepresentations;
        public IDataConverter Converter { get; }

        public Serializer(TypeDescriptorCreator typeDescriptorCreator, IEnumerable<ITypeRepresentation> typeRepresentations, IDataConverter converter = null)
        {
            serializers = new ConcurrentDictionary<Type, IForType>();
            TypeDescriptorCreator = typeDescriptorCreator;
            this.typeRepresentations = typeRepresentations.ToArray();
            Converter = converter ?? DataConverter.Default;
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
            serializers[typeof(T)] = new Typed<T>.Simple(this);
        }
        public SItem Serialize<T>(C context, T item)
        {
            return new SObject();
        }
        public SItem Serialize(C context, object item)
        {
            return new SObject();
        }
        public SItem Serialize(C context, Type type, object item)
        {
            return new SObject();
        }
        public T Deserialize<T>(C context, SItem item)
        {
            return default;
        }
        public object Deserialize(C context, Type type, SItem item)
        {
            return null;
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
