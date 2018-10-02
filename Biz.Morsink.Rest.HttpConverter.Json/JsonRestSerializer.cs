﻿using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.HttpConverter.Json;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SerializationContext = Biz.Morsink.Rest.Serialization.SerializationContext;
using Biz.Morsink.Rest.Utils;
namespace Biz.Morsink.Rest.HttpConverter
{
    public class JsonRestSerializer : Serializer<SerializationContext>
    {
        private readonly IOptions<JsonHttpConverterOptions> jsonOptions;
        private readonly IRestIdentityProvider identityProvider;
        private readonly IdentityRepresentation identityRepresentation;

        public JsonRestSerializer(
            ITypeDescriptorCreator typeDescriptorCreator,
            IOptions<JsonHttpConverterOptions> jsonOptions,
            IRestIdentityProvider identityProvider,
            IRestPrefixContainerAccessor prefixContainerAccessor,
            IOptions<RestAspNetCoreOptions> options,
            ICurrentHttpRestConverterAccessor currentHttpRestConverterAccessor,
            IDataConverter converter = null)
            : base(new DecoratedTypeDescriptorCreator(typeDescriptorCreator),
                 converter)
        {
            var tdc = (DecoratedTypeDescriptorCreator)TypeDescriptorCreator;
            tdc.Decorate(new TypeDescriptorJsonRepresentation(tdc, jsonOptions));
            this.jsonOptions = jsonOptions;
            this.identityProvider = identityProvider;
            identityRepresentation = new IdentityRepresentation(identityProvider, prefixContainerAccessor, options, currentHttpRestConverterAccessor);
        }
        protected override IForType CreateSerializer(Type ty)
        {
            var ser = base.CreateSerializer(ty);
            if (typeof(IIdentity).IsAssignableFrom(ty))
                return CreateIdentitySerializer(ser, ty);
            else
                return ser;
        }

        private IForType CreateIdentitySerializer(IForType ser, Type ty)
            => (IForType)Activator.CreateInstance(typeof(IdentityType<>).MakeGenericType(ty), this, ser);

        private class IdentityType<T> : Typed<T>
        {
            private readonly Typed<T> inner;

            public new JsonRestSerializer Parent => (JsonRestSerializer)base.Parent;
            public IdentityType(JsonRestSerializer parent, Typed<T> inner) : base(parent)
            {
                this.inner = inner;
            }

            public override T Deserialize(SerializationContext context, SItem item)
            {
                if (item is SObject obj)
                {
                    var href = obj.Properties.FirstOrDefault(p => p.Name.Equals("href", StringComparison.InvariantCultureIgnoreCase));
                    if (href != null)
                    {
                        var rep = Parent.Deserialize(context, Parent.identityRepresentation.GetRepresentationType(typeof(T)), item);
                        return (T)(rep == null ? null : ((ITypeRepresentation)Parent.identityRepresentation).GetRepresentable(rep));
                    }
                }
                // fall through:
                return inner.Deserialize(context, item);
            }

            public override SItem Serialize(SerializationContext context, T item)
            {
                if (Parent.jsonOptions.Value.EmbedEmbeddings
                    && item is IIdentity id
                    && context.TryGetEmbedding(id, out var embedding))
                    return Parent.Serialize(context.Without(id), embedding);
                else
                    return inner.Serialize(context, item);
            }
        }

        public void WriteJson(JsonWriter writer, object item, JsonSerializer serializer)
        {
            var sitem = Serialize(SerializationContext.Create(identityProvider), item);
            WriteJson(writer, sitem, serializer);
        }
        public void WriteJson(JsonWriter writer, SItem item, JsonSerializer serializer)
        {
            switch (item)
            {
                case SObject obj:
                    WriteJson(writer, obj, serializer);
                    break;
                case SValue val:
                    WriteJson(writer, val, serializer);
                    break;
                case SArray arr:
                    WriteJson(writer, arr, serializer);
                    break;
            }
        }
        public void WriteJson(JsonWriter writer, SObject item, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var prop in item.Properties)
            {
                var propName = prop.Format == SFormat.Literal ? prop.Name : Casing(prop.Name);
                writer.WritePropertyName(propName);
                WriteJson(writer, prop.Token, serializer);
            }
            writer.WriteEndObject();
        }
        public void WriteJson(JsonWriter writer, SValue item, JsonSerializer serializer)
        {
            writer.WriteValue(item.Value);
        }
        public void WriteJson(JsonWriter writer, SArray item, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var val in item.Content)
                WriteJson(writer, val, serializer);
            writer.WriteEndArray();
        }
        public SItem ReadJson(JsonReader reader)
        {
            if(reader.TokenType == JsonToken.None)
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
                            reader.Read();
                        }
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
        public object ReadJson(JsonReader reader, Type type)
        {
            return Deserialize(Serialization.SerializationContext.Create(identityProvider), type, ReadJson(reader));
        }
        public T ReadJson<T>(JsonReader reader)
        {
            return Deserialize<T>(Serialization.SerializationContext.Create(identityProvider), ReadJson(reader));
        }
        private string Casing(string str)
        {
            return jsonOptions.Value.NamingStrategy.GetPropertyName(str, false);
        }
    }
}