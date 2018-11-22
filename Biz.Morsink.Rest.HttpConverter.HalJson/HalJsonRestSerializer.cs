using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SerializationContext = Biz.Morsink.Rest.Serialization.SerializationContext;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    /// <summary>
    /// A serializer for the HAL Json media type.
    /// </summary>
    public class HalJsonRestSerializer : Serializer<SerializationContext>
    {
        private readonly IOptions<HalJsonConverterOptions> halOptions;
        private readonly IRestIdentityProvider identityProvider;
        private readonly JsonSerializer jsonSerializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDescriptorCreator">A type descriptor creator.</param>
        /// <param name="halOptions">Options for the HAL Json media type.</param>
        /// <param name="identityProvider">A Rest identity provider.</param>
        /// <param name="converter">An optional data converter.</param>
        public HalJsonRestSerializer(
            ITypeDescriptorCreator typeDescriptorCreator,
            IOptions<HalJsonConverterOptions> halOptions,
            IRestIdentityProvider identityProvider,
            IDataConverter converter = null)
            : base(new DecoratedTypeDescriptorCreator(typeDescriptorCreator)
                  .Decorate(tdc => new ITypeRepresentation[]
                  {
                      SimpleTypeDescriptorRepresentation.Instance
                  }), converter)
        {
            this.halOptions = halOptions;
            this.identityProvider = identityProvider;
            jsonSerializer = JsonSerializer.Create(halOptions.Value.SerializerSettings);
        }
        protected override IForType CreateSerializer(Type ty)
        {
            if (typeof(IRestValue).IsAssignableFrom(ty))
                return CreateRestValueSerializer(ty);
            var ser = base.CreateSerializer(ty);
            if (typeof(IIdentity).IsAssignableFrom(ty))
                return CreateIdentitySerializer(ser, ty);
            else if (typeof(IHasIdentity).IsAssignableFrom(ty))
                return CreateHasIdentitySerializer(ser, ty);
            else
                return ser;
        }

        private IForType CreateRestValueSerializer(Type ty)
            => (IForType)Activator.CreateInstance(typeof(RestValueType<>).MakeGenericType(ty), this);
        private IForType CreateIdentitySerializer(IForType ser, Type ty)
            => (IForType)Activator.CreateInstance(typeof(IdentityType<>).MakeGenericType(ty), this, ser);
        private IForType CreateHasIdentitySerializer(IForType ser, Type ty)
            => (IForType)Activator.CreateInstance(typeof(HasIdentityType<>).MakeGenericType(ty), this, ser);

        private class IdentityType<T> : Typed<T>
        {
            private readonly Typed<T> inner;

            public new HalJsonRestSerializer Parent => (HalJsonRestSerializer)base.Parent;
            public IdentityType(HalJsonRestSerializer parent, Typed<T> inner) : base(parent)
            {
                this.inner = inner;
            }

            public override T Deserialize(SerializationContext context, SItem item)
            {
                return inner.Deserialize(context, item);
            }

            public override SItem Serialize(SerializationContext context, T item)
            {
                return inner.Serialize(context, item);
            }
        }
        private class RestValueType<T> : Typed<T>
            where T : IRestValue
        {
            public new HalJsonRestSerializer Parent => (HalJsonRestSerializer)base.Parent;
            public RestValueType(HalJsonRestSerializer parent) : base(parent)
            {
            }
            public override T Deserialize(SerializationContext context, SItem item)
            {
                throw new NotSupportedException();
            }
            public override SItem Serialize(SerializationContext context, T item)
            {

                var res = (SObject)Parent.Serialize(context.With(item), item.Value);
                var ctxWithItem = context.With(item);
                res = new SObject(res.Properties
                    .Concat(new[] {
                        new SProperty("_links", serializeLinks()),
                        new SProperty("_embedded", new SObject(
                            item.Embeddings.GroupBy(e => e.Reltype)
                            .Select(g => (reltype:g.Key,items:g,num:g.Count()))
                            .Select(t => new SProperty(t.reltype, t.num==1 
                                ? serializeEmbedding(t.items.First()) 
                                : new SArray(t.items.Select(serializeEmbedding)))))
                            )
                        }));
                return res;
                SItem serializeEmbedding(Embedding o)
                {
                    var ctx = o.Object is IHasIdentity hid
                        ? ctxWithItem.Without(hid.Id)
                        : ctxWithItem;
                    return Parent.Serialize(ctx, o.Object);
                }
                SObject serializeLinks()
                {
                    var props = from l in item.Links
                                group l by l.RelType into g
                                let num = g.Count()
                                select num == 1
                                    ? new SProperty(g.First().RelType, Parent.Serialize(context, g.First().Target))
                                    : new SProperty(g.First().RelType, 
                                        new SArray(g.Select(x => Parent.Serialize(context, x.Target))));
                    return new SObject(props);
                }
            }
        }
        private class HasIdentityType<T> : Typed<T>
            where T : IHasIdentity
        {
            private readonly Typed<T> inner;
            public new HalJsonRestSerializer Parent => (HalJsonRestSerializer)base.Parent;
            public HasIdentityType(HalJsonRestSerializer parent, Typed<T> inner) : base(parent)
            {
                this.inner = inner;
            }
            public override T Deserialize(SerializationContext context, SItem item)
            {
                return inner.Deserialize(context, item);
            }
            public override SItem Serialize(SerializationContext context, T item)
            {
                if (context.IsInParentChain(item.Id) || context.TryGetEmbedding(item.Id, out var _))
                    return Parent.Serialize(context.WithParent(item.Id), item.Id);
                else
                    return inner.Serialize(context.WithParent(item.Id), item);
            }
        }
        /// <summary>
        /// Convert the object to intermediate format and serialize to a JsonWriter.
        /// </summary>
        /// <param name="writer">A JsonWriter</param>
        /// <param name="item">The object to serialize.</param>
        public void WriteJson(JsonWriter writer, object item)
        {
            var sitem = Serialize(SerializationContext.Create(identityProvider), item);
            WriteJson(writer, sitem);
        }
        /// <summary>
        /// Serialize an SItem to a JsonWriter.
        /// </summary>
        /// <param name="writer">The JsonWriter.</param>
        /// <param name="item">An SItem object to serialize.</param>
        public void WriteJson(JsonWriter writer, SItem item)
        {
            switch (item)
            {
                case SObject obj:
                    WriteJson(writer, obj);
                    break;
                case SValue val:
                    WriteJson(writer, val);
                    break;
                case SArray arr:
                    WriteJson(writer, arr);
                    break;
            }
        }
        /// <summary>
        /// Serialize an SObject to a JsonWriter.
        /// </summary>
        /// <param name="writer">The JsonWriter.</param>
        /// <param name="item">An SObject object to serialize.</param>
        public void WriteJson(JsonWriter writer, SObject item)
        {
            writer.WriteStartObject();
            foreach (var prop in item.Properties)
            {
                if (!(prop.Token is SValue s) || s.Value != null || halOptions.Value.SerializerSettings.NullValueHandling == NullValueHandling.Include)
                {
                    var propName = prop.Format == SFormat.Literal ? prop.Name : Casing(prop.Name);
                    writer.WritePropertyName(propName);
                    WriteJson(writer, prop.Token);
                }
            }
            writer.WriteEndObject();
        }
        /// <summary>
        /// Serialize an SValue to a JsonWriter.
        /// </summary>
        /// <param name="writer">The JsonWriter.</param>
        /// <param name="item">An SValue object to serialize.</param>
        public void WriteJson(JsonWriter writer, SValue item)
        {
            jsonSerializer.Serialize(writer, item.Value);
        }
        /// <summary>
        /// Serialize an SArray to a JsonWriter.
        /// </summary>
        /// <param name="writer">The JsonWriter.</param>
        /// <param name="item">An SArray object to serialize.</param>
        public void WriteJson(JsonWriter writer, SArray item)
        {
            writer.WriteStartArray();
            foreach (var val in item.Content)
                WriteJson(writer, val);
            writer.WriteEndArray();
        }

        /// <summary>
        /// Deserialize an SItem from a JsonReader.
        /// </summary>
        /// <param name="reader">The JsonReader.</param>
        /// <returns>An SItem representing the content of the reader.</returns>
        public SItem ReadJson(JsonReader reader)
        {
            if (reader.TokenType == JsonToken.None)
                reader.Read();
            return doRead();

            SItem doRead()
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        var props = new List<SProperty>();
                        reader.Read();
                        while (reader.TokenType == JsonToken.PropertyName)
                        {
                            var prop = reader.Value.ToString();
                            reader.Read();
                            var val = doRead();
                            props.Add(new SProperty(prop, val));
                        }
                        if (reader.TokenType != JsonToken.EndObject)
                            throw new JsonSerializationException("No EndObject token.");
                        reader.Read();
                        return new SObject(props);
                    case JsonToken.StartArray:
                        var vals = new List<SItem>();
                        reader.Read();
                        while (reader.TokenType != JsonToken.EndArray)
                        {
                            var val = doRead();
                            vals.Add(val);
                        }
                        reader.Read();
                        return new SArray(vals);
                    case JsonToken.Null:
                        reader.Read();
                        return SValue.Null;
                    default:
                        if (reader.Value == null)
                            throw new JsonSerializationException("Unknown token");
                        var value = reader.Value;
                        reader.Read();
                        return new SValue(value);
                }
            }
        }
        /// <summary>
        /// Deserialize an object from a JsonReader.
        /// </summary>
        /// <param name="reader">The JsonReader.</param>
        /// <param name="type">The type of object to read.</param>
        /// <returns>An object representing the content of the reader.</returns>    
        public object ReadJson(JsonReader reader, Type type)
        {
            return Deserialize(Serialization.SerializationContext.Create(identityProvider), type, ReadJson(reader));
        }
        /// <summary>
        /// Deserialize an object from a JsonReader.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="reader">The JsonReader.</param>
        /// <returns>An object representing the content of the reader.</returns>    
        public T ReadJson<T>(JsonReader reader)
        {
            return Deserialize<T>(Serialization.SerializationContext.Create(identityProvider), ReadJson(reader));
        }
        private string Casing(string str)
        {
            return halOptions.Value.NamingStrategy.GetPropertyName(str, false);
        }
    }
}
