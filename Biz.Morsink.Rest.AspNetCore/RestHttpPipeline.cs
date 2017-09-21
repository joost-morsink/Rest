using Biz.Morsink.Rest.Metadata;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// An HTTP pipeline for Rest requests.
    /// </summary>
    public class RestHttpPipeline : IRestHttpPipeline
    {
        private readonly ImmutableList<Func<RestRequestDelegate, RestRequestDelegate>> middlewares;

        /// <summary>
        /// Creates an empty RestHttpPipeline
        /// </summary>
        /// <returns>An empty RestHttpPipeline</returns>
        public static RestHttpPipeline Create() => new RestHttpPipeline(ImmutableList<Func<RestRequestDelegate, RestRequestDelegate>>.Empty);
        private RestHttpPipeline(ImmutableList<Func<RestRequestDelegate, RestRequestDelegate>> middlewares)
        {
            this.middlewares = middlewares;
        }

        /// <summary>
        /// Uses a middleware component on the RestHttpPipeline.
        /// </summary>
        /// <param name="middleware">A function containing the middleware code.</param>
        /// <returns>A new RestHttpPipeline with the added specified middleware.</returns>
        public RestHttpPipeline Use(Func<RestRequestDelegate, RestRequestDelegate> middleware)
            => new RestHttpPipeline(middlewares.Add(middleware));

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

        IRestHttpPipeline IRestHttpPipeline.Use(Func<RestRequestDelegate, RestRequestDelegate> middleware)
            => Use(middleware);
    }
    /// <summary>
    /// Interface for HTTP pipelines for Rest requests.
    /// </summary>
    public interface IRestHttpPipeline
    {
        /// <summary>
        /// Gets an actual RestRequestDelegate by setting the 'core' Rest request handler.
        /// </summary>
        /// <param name="handler">An implementation of the IRestRequestHandler.</param>
        /// <returns>A RestRequestDelegate that incorporates all the logic for the middleware and the core request handler.</returns>
        RestRequestDelegate GetRequestDelegate(IRestRequestHandler handler);
        /// <summary>
        /// Uses a middleware component on the IRestHttpPipeline.
        /// </summary>
        /// <param name="middleware">A function containing the middleware code.</param>
        /// <returns>A new IRestHttpPipeline with the added specified middleware.</returns>
        IRestHttpPipeline Use(Func<RestRequestDelegate, RestRequestDelegate> middleware);
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
    public static class RestHttpPipelineExt
    {
        /// <summary>
        /// Adds a middleware component to the RestHttpPipeline that implements metadata for caching through HTTP headers.
        /// </summary>
        /// <param name="pipeline">The Rest HTTP pipeline.</param>
        /// <returns>A new Rest HTTP pipeline containing the caching middleware.</returns>
        public static IRestHttpPipeline UseCaching(this IRestHttpPipeline pipeline)
            => pipeline.Use(next => async (context, req, conv) =>
            {
                RestResponse response;
                if (context.Request.Headers.ContainsKey("If-None-Match"))
                {
                    var reqCache = new RequestCache { Token = context.Request.Headers["If-None-Match"][0] };
                    response = await next(context, req.AddMetadata(reqCache), conv);
                }
                else
                    response = await next(context, req, conv);
                if (response.Metadata.TryGet<ResponseCache>(out var cache))
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

    }
}
