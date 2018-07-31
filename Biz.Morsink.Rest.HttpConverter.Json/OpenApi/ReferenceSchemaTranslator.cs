using Biz.Morsink.Rest.AspNetCore.OpenApi;
using Biz.Morsink.Rest.HttpConverter.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json.OpenApi
{
    /// <summary>
    /// A Json schema translator for the reference class.
    /// This class in necessary to serialize the '$ref' propertyname.
    /// </summary>
    public class ReferenceSchemaTranslator : IJsonSchemaTranslator
    {
        public JsonConverter GetConverter(Type type)
            => typeof(Reference) == type ? Converter.Instance : null;

        public JsonSchema GetSchema(Type type)
            => type == typeof(Reference)
            ? new JsonSchema(new JObject(
                new JProperty("type", "object"),
                new JProperty("properties", new JObject(
                    new JProperty("$ref", new JObject(
                        new JProperty("type", "string"))))),
                new JProperty("required", new JArray("$ref"))))
            : null;
        
        private class Converter : JsonConverter
        {
            public static Converter Instance { get; } = new Converter();
            public override bool CanConvert(Type objectType)
                => typeof(Reference) == objectType;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jobj = serializer.Deserialize<JObject>(reader);
                if (jobj["$ref"] == null)
                    throw new JsonSerializationException("$ref property expected.");
                return new Reference { Ref = jobj.Value<string>("$ref") };
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var r = (Reference)value;
                writer.WriteStartObject();
                writer.WritePropertyName("$ref");
                writer.WriteValue(r.Ref);
                writer.WriteEndObject();
            }
        }
    }
}
