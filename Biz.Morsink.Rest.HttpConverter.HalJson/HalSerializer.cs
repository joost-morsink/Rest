﻿using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {
        private readonly ConcurrentDictionary<Type, IForType> serializers;
        private readonly IDataConverter converter;
        private readonly ITypeRepresentation[] representations;

        public HalSerializer(TypeDescriptorCreator typeDescriptorCreator, IDataConverter converter, IEnumerable<ITypeRepresentation> typeRepresentations)
        {
            serializers = new ConcurrentDictionary<Type, IForType>();
            this.converter = converter;
            representations = typeRepresentations.ToArray();
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

        public JToken Serialize(Type type, HalContext context, object item)
        {
            if (item == null)
                return null;
            var serializer = GetSerializerForType(type);
            return serializer.Serialize(context, item);
        }
        public JToken Serialize<T>(HalContext context, T item)
        {
            if (item == null)
                return null;
            var serializer = GetSerializerForType<T>();
            return serializer.Serialize(context, item);
        }
        public object Deserialize(Type type, HalContext context, JToken token)
        {
            var serializer = GetSerializerForType(type);
            return serializer.Deserialize(context, token);
        }
        public T Deserialize<T>(HalContext context, JToken token)
        {
            var serializer = GetSerializerForType<T>();
            return serializer.Deserialize(context, token);
        }
        public IForType GetSerializerForType(Type type)
            => serializers.GetOrAdd(type, Get);
        public Typed<T> GetSerializerForType<T>()
            => (Typed<T>)GetSerializerForType(typeof(T));
        private IForType Get(Type t)
        {
            return null;
        }
        public interface IForType
        {
            Type Type { get; }
            JToken Serialize(HalContext context, object item);
            object Deserialize(HalContext context, JToken token);
        }

    }
}
