using Biz.Morsink.Rest.AspNetCore.OpenApi;
using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json.OpenApi
{
    /// <summary>
    /// JsonConverter for OrReference&lt;T&gt;
    /// </summary>
    /// <typeparam name="T">The generic parameter for OrReference.</typeparam>
    public class OrReferenceConverter<T> : JsonConverter, IJsonSchemaTranslator<OrReference<T>>
    {
        private readonly IJsonSchemaProvider schemaProvider;
        private readonly TypeDescriptorCreator typeDescriptorCreator;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schemaProvider">A schema provider.</param>
        /// <param name="typeDescriptorCreator">A type descriptor creator.</param>
        public OrReferenceConverter(IJsonSchemaProvider schemaProvider, TypeDescriptorCreator typeDescriptorCreator)
        {
            this.schemaProvider = schemaProvider;
            this.typeDescriptorCreator = typeDescriptorCreator;
        }
        public override bool CanConvert(Type objectType)
            => typeof(OrReference<T>) == objectType;

        public JsonConverter GetConverter(Type type)
            => type == typeof(OrReference<T>) ? this : null;

        public JsonSchema GetSchema(Type type)
        {
            if (type != typeof(OrReference<T>))
                return null;
            var schema = schemaProvider.GetSchema(typeof(T));
            return new JsonSchema(new JObject(
                new JProperty("oneOf", new JArray(
                    new JObject(
                        new JProperty("properties",
                            new JObject(
                                new JProperty("$ref",
                                    new JObject(new JProperty("type", "string")))))),
                    schema.Schema))));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JToken.ReadFrom(reader) as JObject;
            if (o.Property("$ref") == null)
            {
                using (var rdr = o.CreateReader())
                    return new OrReference<T>.ItemImpl(serializer.Deserialize<T>(rdr));
            }
            else
                return new OrReference<T>.ReferenceImpl(new Reference { Ref = o.Property("$ref").Value<string>() });
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var x = (OrReference<T>)value;
            if (x is OrReference<T>.ReferenceImpl r)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("$ref");
                writer.WriteValue(r.Reference.Ref);
                writer.WriteEndObject();
            }
            else if(x is OrReference<T>.ItemImpl i)
                serializer.Serialize(writer, i.Item);
        }
    }
}
