using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Interface for components that handle a serialization format for Rest over HTTP.
    /// </summary>
    public interface IHttpRestConverter
    {
        /// <summary>
        /// Determines if the converter applies to the given HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext associated with the HTTP Request.</param>
        /// <returns>True if this converter is applicable to the context.</returns>
        bool Applies(HttpContext context);
        /// <summary>
        /// A converter is able to manipulate the RestRequest using this method.
        /// </summary>
        /// <param name="req">The RestRequest extracted from the HttpRequest.</param>
        /// <param name="context">The HttpContext for the request.</param>
        /// <returns>A optionally mutated RestRequest.</returns>
        RestRequest ManipulateRequest(RestRequest req, HttpContext context);
        /// <summary>
        /// A converter is able to parse an HTTP body if it knows the type of data to expect. 
        /// </summary>
        /// <param name="t">The expected type of the body.</param>
        /// <param name="body">A byte array containing the raw HTTP body.</param>
        /// <returns>A parsed object.</returns>
        object ParseBody(Type t, byte[] body);
        /// <summary>
        /// Asynchronously serializes a response to an HttpResponse.
        /// </summary>
        /// <param name="response">The RestResponse that must be serialized over HTTP.</param>
        /// <param name="context">The HttpContext for the request.</param>
        /// <returns>A Task describing the asynchronous progress of the serialization.</returns>
        Task SerializeResponse(RestResponse response, HttpContext context);
        /// <summary>
        /// Gets a boolean indicating if Curies are supported by this IHttpRestConverter.
        /// </summary>
        bool SupportsCuries { get; }
    }
}
