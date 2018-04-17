using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// This class represents the description of a response.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// The description of the response.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// A dictionary of headers for the response.
        /// </summary>
        public Dictionary<string, OrReference<Header>> Headers { get; set; } = new Dictionary<string, OrReference<Header>>();
        /// <summary>
        /// A dictionary containing content descriptors for media types. 
        /// In practice application/json will be used.
        /// </summary>
        public Dictionary<string, Content> Content { get; set; } = new Dictionary<string, Content>();
    }
}
