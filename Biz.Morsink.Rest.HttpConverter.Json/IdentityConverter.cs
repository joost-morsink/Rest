using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// JsonConverter for IIdentity values.
    /// </summary>
    public class IdentityConverter : JsonConverter , IJsonSchemaTranslator<IIdentity>
    {
        private readonly IRestIdentityProvider idProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="idProvider">A Rest identity provider, responsible for 'path' conversions.</param>
        public IdentityConverter(IRestIdentityProvider idProvider)
        {
            this.idProvider = idProvider;
        }
        public override bool CanConvert(Type objectType)
            => typeof(IIdentity).IsAssignableFrom(objectType);

        /// <summary>
        /// Read not supported yet.
        /// </summary>
        public override bool CanRead => false;
        /// <summary>
        /// Returns true, the converter is able to write IIdentity values.
        /// </summary>
        public override bool CanWrite => true;

        Type ISchemaTranslator<JsonSchema>.ForType => typeof(IIdentity);

        /// <summary>
        /// Reads an IIdentity value from the stream.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JObject.Load(reader);
            var href = o["href"].Value<string>();
            if (href == null)
                return existingValue;
            var idval = idProvider.Parse(href, objectType != typeof(IIdentity));
            return idval != null && objectType.IsAssignableFrom(idval.GetType()) ? idval : existingValue;
        }
        /// <summary>
        /// Writes a value of type IIdentity to a JsonWriter.
        /// </summary
        /// <param name="writer">The destination JsonWriter.</param>
        /// <param name="value">The value of IIdentity to write.</param>
        /// <param name="serializer">An instance of a JsonSerializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var id = (IIdentity)value;
            var path = idProvider.ToPath(id);
            writer.WriteStartObject();
            writer.WritePropertyName("href");
            writer.WriteValue(path);
            writer.WriteEndObject();
        }

        JsonConverter IJsonSchemaTranslator.GetConverter()
            => this;
        JsonSchema ISchemaTranslator<JsonSchema>.GetSchema()
            => new JsonSchema(new JObject(new JProperty("properties", new JObject(new JProperty("href", new JObject(new JProperty("type", "string")))))));
    }
}
