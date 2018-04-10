using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// A schema definition for inclusion in OpenAPI Specification version 3.0 documents.
    /// Only simple schema's are supported by this library.
    /// More complex schema's need to be referenced.
    /// </summary>
    public class Schema
    {
        /// <summary>
        /// Contains the type of the object.
        /// </summary>
        public string Type { get; set; }
    }
}
