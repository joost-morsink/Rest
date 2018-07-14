using Biz.Morsink.Rest.HttpConverter.Json;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Class to convert RestValues to Json.
    /// </summary>
    public class RestValueConverter : JsonConverter, IJsonSchemaTranslator
    {
        private readonly IJsonSchemaProvider schemaProvider;
        private readonly TypeDescriptorCreator typeDescriptorCreator;
        private readonly IOptions<JsonHttpConverterOptions> options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDescriptorCreator">A Type descriptor creator.</param>
        /// <param name="schemaProvider">A Json schema provider.</param>
        /// <param name="options">Options for the Json Http converter component.</param>
        public RestValueConverter(TypeDescriptorCreator typeDescriptorCreator, IJsonSchemaProvider schemaProvider, IOptions<JsonHttpConverterOptions> options) {

            this.schemaProvider = schemaProvider;
            this.typeDescriptorCreator = typeDescriptorCreator;
            this.options = options;
        }

        public Type ForType
            => typeof(IRestValue);
               
        public override bool CanConvert(Type objectType)
            => ForType.IsAssignableFrom(objectType);

        public JsonConverter GetConverter()
            => this;

        /// <summary>
        /// Getting the schema is not supported, because the value type is unknown.
        /// </summary>
        /// <returns>Throws a NotSupportedException.</returns>
        public JsonSchema GetSchema()
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => false;
        /// <summary>
        /// Reading is not permitted.
        /// </summary>
        /// <returns>Throws a NotImplementedException.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Writes a Rest value to a Json stream.
        /// </summary>
        /// <param name="writer">The Json writer.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="serializer">The serializer to use for serialization.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            var type = value.GetType();
            if (!typeof(IRestValue).IsAssignableFrom(type))
                throw new ArgumentException("Type should be assignable to IRestValue", nameof(type));
            var valueType = type.GetGeneric(typeof(RestValue<>)) ?? typeof(object);

            var opts = options.Value;
            var rv = (IRestValue)value;
            if (opts.LinkLocation != null) {
                var o = JObject.FromObject(rv.Value, serializer);
                o.Add(new JProperty(opts.LinkLocation, new JArray(rv.Links.Select(l => JObject.FromObject(l,serializer)))));
                serializer.Serialize(writer, o);
            }
            else
                serializer.Serialize(writer, rv.Value, rv.ValueType);
            
        }
    }
}
