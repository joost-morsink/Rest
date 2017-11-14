using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// A Json schema translator implementation for dynamic objects.
    /// </summary>
    public class DynamicConverter : IJsonSchemaTranslator<ExpandoObject>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DynamicConverter()
        {

        }

        /// <summary>
        /// Returns the type of ExpandoObject.
        /// </summary>
        public Type ForType => typeof(ExpandoObject);

        /// <summary>
        /// Returns null.
        /// Newtonsoft.Json handles serialization and deserialization of ExpandoObjects already.
        /// </summary>
        /// <returns>null.</returns>
        public JsonConverter GetConverter()
            => null;

        /// <summary>
        /// Gets a schema for a generic (dynamic) object.
        /// </summary>
        /// <returns></returns>
        public JsonSchema GetSchema()
            => new JsonSchema(new JObject(
                new JProperty("$schema", JsonSchemaTypeDescriptorVisitor.JSON_SCHEMA_VERSION),
                new JProperty("properties", new JObject()),
                new JProperty("required", new JArray())));
    }
}
