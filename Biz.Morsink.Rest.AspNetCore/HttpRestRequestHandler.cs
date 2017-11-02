using Biz.Morsink.Rest.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// An HTTP pipeline for Rest requests.
    /// </summary>
    public class HttpRestRequestHandler : IHttpRestRequestHandler
    {
        private readonly ImmutableList<Func<RestRequestDelegate, RestRequestDelegate>> middlewares;

        /// <summary>
        /// Creates an empty HttpRestRequestHandler
        /// </summary>
        /// <returns>An empty HttpRestRequestHandler</returns>
        public static HttpRestRequestHandler Create() => new HttpRestRequestHandler(ImmutableList<Func<RestRequestDelegate, RestRequestDelegate>>.Empty);
        private HttpRestRequestHandler(ImmutableList<Func<RestRequestDelegate, RestRequestDelegate>> middlewares)
        {
            this.middlewares = middlewares;
        }

        /// <summary>
        /// Uses a middleware component on the HttpRestRequestHandler.
        /// </summary>
        /// <param name="middleware">A function containing the middleware code.</param>
        /// <returns>A new HttpRestRequestHandler with the added specified middleware.</returns>
        public HttpRestRequestHandler Use(Func<RestRequestDelegate, RestRequestDelegate> middleware)
            => new HttpRestRequestHandler(middlewares.Add(middleware));

        /// <summary>
        /// Gets an actual RestRequestDelegate by setting the 'core' Rest request handler.
        /// </summary>
        /// <param name="handler">An implementation of the IRestRequestHandler.</param>
        /// <returns>A RestRequestDelegate that incorporates all the logic for the middleware and the core request handler.</returns>
        public RestRequestDelegate GetRequestDelegate(IRestRequestHandler handler)
        {
            RestRequestDelegate result = (context, req, conv) => handler.HandleRequest(req.AddMetadata(context.RequestServices));
            foreach (var m in middlewares.Reverse())
                result = m(result);
            return result;
        }

        IHttpRestRequestHandler IHttpRestRequestHandler.Use(Func<RestRequestDelegate, RestRequestDelegate> middleware)
            => Use(middleware);
    }
}
