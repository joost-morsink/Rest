using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// This class represents a description for the request body.
    /// </summary>
    public class RequestBody
    {
        /// <summary>
        /// A description of the request body.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// A dictionary containing content descriptors for media types. 
        /// In practice application/json will be used.
        /// </summary>
        public Dictionary<string, Content> Content { get; set; } = new Dictionary<string, Content>();
        /// <summary>
        /// Indicates whether the request body is required.
        /// </summary>
        public bool Required { get; set; }
    }
}