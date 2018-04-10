using Newtonsoft.Json;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// This class contains the API information regarding a path.
    /// </summary>
    public class Path
    { 
        /// <summary>
        /// A reference.
        /// </summary>
        public string Ref {get;set;}
        /// <summary>
        /// The summary for this path.
        /// </summary>
        public string Summary { get; set; }
        /// <summary>
        /// The description for this path.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The Get operation.
        /// </summary>
        public Operation Get { get; set; }
        /// <summary>
        /// The Put operation.
        /// </summary>
        public Operation Put { get; set; }
        /// <summary>
        /// The Post operation.
        /// </summary>
        public Operation Post { get; set; }
        /// <summary>
        /// The Patch operation.
        /// </summary>
        public Operation Patch { get; set; }
        /// <summary>
        /// The Delete operation.
        /// </summary>
        public Operation Delete { get; set; }
        /// <summary>
        /// The Options operation.
        /// </summary>
        public Operation Options { get; set; }
    }
}