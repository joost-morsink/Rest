using Biz.Morsink.Rest.Metadata;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    public class RestHttpPipeline : IRestHttpPipeline
    {
        private readonly ImmutableList<Func<RestRequestDelegate, RestRequestDelegate>> middlewares;

        public static RestHttpPipeline Create() => new RestHttpPipeline(ImmutableList<Func<RestRequestDelegate, RestRequestDelegate>>.Empty);
        private RestHttpPipeline(ImmutableList<Func<RestRequestDelegate, RestRequestDelegate>> middlewares)
        {
            this.middlewares = middlewares;
        }

        public RestHttpPipeline Use(Func<RestRequestDelegate, RestRequestDelegate> middleware)
            => new RestHttpPipeline(middlewares.Add(middleware));

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
    public interface IRestHttpPipeline
    {
        RestRequestDelegate GetRequestDelegate(IRestRequestHandler handler);
        IRestHttpPipeline Use(Func<RestRequestDelegate, RestRequestDelegate> middleware);
    }
    public delegate ValueTask<RestResponse> RestRequestDelegate(HttpContext context, RestRequest req, IHttpRestConverter converter);
    public static class RestHttpPipelineExt
    {
        public static IRestHttpPipeline UseCaching(IRestHttpPipeline pipeline)
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
