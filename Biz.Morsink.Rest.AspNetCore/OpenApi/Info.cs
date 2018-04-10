using System;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// Informational structure for an OpenAPI Specification version 3.0 document.
    /// </summary>
    public class Info
    {
        /// <summary>
        /// The title of the API.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The description of the API.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The version of the API.
        /// </summary>
        public string Version { get; set; }
    }
}