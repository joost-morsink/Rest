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
    public partial class HalSerializer
    {
        private readonly ConcurrentDictionary<Type, IForType> serializers;
        private readonly IDataConverter converter;
        private readonly ITypeRepresentation[] representations;
        private readonly IRestIdentityProvider identityProvider;
        public HalSerializer(TypeDescriptorCreator typeDescriptorCreator, IDataConverter converter, IRestIdentityProvider identityProvider, IEnumerable<ITypeRepresentation> typeRepresentations)
        {
            serializers = new ConcurrentDictionary<Type, IForType>();
            this.converter = converter;
            representations = typeRepresentations.ToArray();
            this.identityProvider = identityProvider;
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
        public JToken Serialize(HalContext context, object item)
        {
            if (item == null)
                return null;
            var serializer = GetSerializerForType(item.GetType());
            return serializer.Serialize(context, item);
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
                else
                    return (IForType)Activator.CreateInstance(typeof(Typed<>.Default).MakeGenericType(t), this);
            }
        }
        public interface IForType
        {
            Type Type { get; }
            JToken Serialize(HalContext context, object item);
            object Deserialize(HalContext context, JToken token);
        }

    }
}
