using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    /// <summary>
    /// This class represents an operation on an API.
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// A summary of what the operation does.
        /// </summary>
        public string Summary { get; set; }
        /// <summary>
        /// A description of what the operation does.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// A list of parameters for the operation.
        /// </summary>
        public List<OrReference<Parameter>> Parameters { get; set; } = new List<OrReference<Parameter>>();
        /// <summary>
        /// The request body description.
        /// </summary>
        public RequestBody RequestBody { get; set; }
        /// <summary>
        /// Possible responses for the operation.
        /// </summary>
        public Dictionary<string, Response> Responses { get; set; } = new Dictionary<string, Response>();
    }
}