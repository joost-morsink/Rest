using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Interface for HTTP pipelines for Rest requests.
    /// </summary>
    public interface IHttpRestRequestHandler
    {
        /// <summary>
        /// Gets an actual RestRequestDelegate by setting the 'core' Rest request handler.
        /// </summary>
        /// <param name="handler">An implementation of the IRestRequestHandler.</param>
        /// <returns>A RestRequestDelegate that incorporates all the logic for the middleware and the core request handler.</returns>
        RestRequestDelegate GetRequestDelegate(IRestRequestHandler handler);
        /// <summary>
        /// Uses a middleware component on the IHttpRestRequestHandler.
        /// </summary>
        /// <param name="middleware">A function containing the middleware code.</param>
        /// <returns>A new IHttpRestRequestHandler with the added specified middleware.</returns>
        IHttpRestRequestHandler Use(Func<RestRequestDelegate, RestRequestDelegate> middleware);
    }
    /// <summary>
    /// Delegate type for Rest request handlers.
    /// </summary>
    /// <param name="context">The HttpContext for the request.</param>
    /// <param name="req">The Rest request.</param>
    /// <param name="converter">The applicable IHttpRestConverter implementation.</param>
    /// <returns>A possibly asynchronous Rest response.</returns>
    public delegate ValueTask<RestResponse> RestRequestDelegate(HttpContext context, RestRequest req, IHttpRestConverter converter);

}
