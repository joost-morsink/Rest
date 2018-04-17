using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Class for conversion between TypeDescriptors and Json.
    /// </summary>
    public class TypeDescriptorConverter : JsonConverter, IJsonSchemaTranslator<TypeDescriptor>
    {
        private readonly IJsonSchemaProvider schemaProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider to lazily evaluate a collection of IJsonSchemaTranslators.</param>
        public TypeDescriptorConverter(IJsonSchemaProvider schemaProvider)
        {
            this.schemaProvider = schemaProvider;
        }
        /// <summary>
        /// This Converter can convert TypeDescriptors.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <returns>True if the object type is assignable to TypeDescriptor.</returns>
        public override bool CanConvert(Type objectType)
            => typeof(TypeDescriptor).IsAssignableFrom(objectType);
        /// <summary>
        /// Not supported: TypeDescriptors map to Schemas and schemas should be readonly.
        /// </summary>
        public override bool CanRead => false;
        /// <summary>
        /// This converter is able to write TypeDescriptors to a JsonWriter.
        /// </summary>
        public override bool CanWrite => true;

        Type ISchemaTranslator<JsonSchema>.ForType => typeof(TypeDescriptor);

        /// <summary>
        /// Not supported: Throws an exception.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes a value of type TypeDescriptor to a JsonWriter.
        /// </summary
        /// <param name="writer">The destination JsonWriter.</param>
        /// <param name="value">The value of TypeDescriptor to write.</param>
        /// <param name="serializer">An instance of a JsonSerializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var typeDescriptor = (TypeDescriptor)value;
            schemaProvider.GetSchema(typeDescriptor).Schema.WriteTo(writer, serializer.Converters.ToArray());
        }

        JsonConverter IJsonSchemaTranslator.GetConverter()
            => this;

        JsonSchema ISchemaTranslator<JsonSchema>.GetSchema()
            => new JsonSchema(new JObject(new JProperty("$ref", JsonSchemaTypeDescriptorVisitor.JSON_SCHEMA_VERSION)));
    }
}
