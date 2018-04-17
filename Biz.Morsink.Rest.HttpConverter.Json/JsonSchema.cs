using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// This type represents Json schemas.
    /// At the moment the schema can be passed to the constructor as a JObject.
    /// It is very probable this will change.
    /// </summary>
    public class JsonSchema
    {
        /// <summary>
        /// Gets the url ($ref) for Json schema (draft-07).
        /// </summary>
        public const string JSON_SCHEMA_VERSION = "http://json-schema.org/draft-07/schema#";

        /// <summary>
        /// Constructor.
        /// </summary>
        public JsonSchema(JObject schema)
        {
            Schema = schema;
        }
        /// <summary>
        /// Gets the schema as a JObject.
        /// </summary>
        public JObject Schema { get; }
    }
}
