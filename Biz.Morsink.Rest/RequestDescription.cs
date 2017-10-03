using Biz.Morsink.Rest.Schema;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class describes a certain type of Rest request.
    /// </summary>
    public class RequestDescription
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="requestBody">A TypeDescriptor for the request body.</param>
        /// <param name="parameters">A TypeDescriptor for the parameters.</param>
        /// <param name="responseBody">A TypeDescriptor for the reponse body.</param>
        public RequestDescription(TypeDescriptor requestBody, TypeDescriptor parameters, TypeDescriptor responseBody)
        {
            RequestBody = requestBody;
            Parameters = parameters;
            ResponseBody = responseBody;
        }
        /// <summary>
        /// Gets a TypeDescriptor for the request body.
        /// </summary>
        public TypeDescriptor RequestBody { get; }
        /// <summary>
        /// Gets a TypeDescriptor for the parameters.
        /// </summary>
        public TypeDescriptor Parameters { get; }
        /// <summary>
        /// Gets a TypeDescriptor for the response body.
        /// </summary>
        public TypeDescriptor ResponseBody { get; }
    }
}
