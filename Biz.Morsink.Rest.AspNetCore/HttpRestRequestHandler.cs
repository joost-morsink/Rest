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
            RestRequestDelegate result = (context, req, conv) => handler.HandleRequest(req);
            foreach (var m in middlewares.Reverse())
                result = m(result);
            return result;
        }

        IHttpRestRequestHandler IHttpRestRequestHandler.Use(Func<RestRequestDelegate, RestRequestDelegate> middleware)
            => Use(middleware);
    }
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
    /// <summary>
    /// Helper class for extension methods.
    /// </summary>
    public static class HttpRestRequestHandlerExt
    {
        /// <summary>
        /// Adds a middleware component to the IHttpRestRequestHandler that implements metadata for caching through HTTP headers.
        /// </summary>
        /// <param name="pipeline">The Rest HTTP pipeline.</param>
        /// <returns>A new IHttpRestRequestHandler containing the caching middleware.</returns>
        public static IHttpRestRequestHandler UseCaching(this IHttpRestRequestHandler pipeline)
            => pipeline.Use(next => async (context, req, conv) =>
            {
                RestResponse response;
                if (context.Request.Headers.ContainsKey("If-None-Match"))
                {
                    var reqCache = new RequestCaching { Token = context.Request.Headers["If-None-Match"][0] };
                    response = await next(context, req.AddMetadata(reqCache), conv);
                }
                else
                    response = await next(context, req, conv);
                if (response.Metadata.TryGet<ResponseCaching>(out var cache))
                {
                    if (!cache.StoreAllowed)
                        context.Response.Headers["Cache-Control"] = "no-store";
                    else if (!cache.CacheAllowed)
                        context.Response.Headers["Cache-Control"] = "no-cache";
                    else
                    {
                        if (cache.Token != null)
                            context.Response.Headers["ETag"] = cache.Token;
                        if (cache.Validity > TimeSpan.Zero)
                            context.Response.Headers["Cache-Control"] = $"{(cache.CachePrivate ? "private," : "")}max-age={(int)cache.Validity.TotalSeconds}";
                    }
                }
                return response;
            });
        /// <summary>
        /// Adds a middleware component to the IHttpRestRequestHandler that implements metadata for capability discovery.
        /// This mainly applies to OPTIONS requests.
        /// </summary>
        /// <param name="pipeline">The Rest HTTP pipeline.</param>
        /// <returns>A new IHttpRestRequestHandler containing the capability discovery middleware.</returns>
        public static IHttpRestRequestHandler UseCapabilityDiscovery(this IHttpRestRequestHandler pipeline)
            => pipeline.Use(next => async (context, req, conv) =>
            {
                var response = await next(context, req, conv);
                response.Metadata.Execute<Capabilities>(caps =>
                    context.Response.Headers["Allow"] = string.Join(",", caps.Methods));// new StringValues(caps.Methods.ToArray())));
                return response;
            });
        /// <summary>
        /// Adds a middleware component to the IHttpRestRequestHandler that implements metadata for the location header.
        /// </summary>
        /// <param name="pipeline">The Rest HTTP pipeline.</param>
        /// <param name="serviceProvider">A service provider is needed to resolve the Address to a Path.</param>
        /// <returns>A new IHttpRestRequestHandler containing location header middleware.</returns>
        public static IHttpRestRequestHandler UseLocationHeader(this IHttpRestRequestHandler pipeline, IServiceProvider serviceProvider)
            => pipeline.Use(next => async (context, req, conv) =>
            {
                var response = await next(context, req, conv);
                response.Metadata.Execute<Location>(loc =>
                {
                    var idProv = serviceProvider.GetRequiredService<IRestIdentityProvider>();
                    context.Response.Headers["Location"] = idProv.ToPath(loc.Address);
                });
                return response;
            });
    }
}
