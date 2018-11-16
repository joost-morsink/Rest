using Biz.Morsink.Rest.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Helper class for extension methods.
    /// </summary>
    public static class HttpRestRequestHandlerExt
    {
        private static VersionToken ParseToken(string token)
        {
            if (token.StartsWith("W/\"") && token.EndsWith("\"") && token.Length >= 4)
                return new VersionToken { Token = token.Substring(3, token.Length - 4), IsStrong = false };
            else if (token.StartsWith("\"") && token.EndsWith("\"") && token.Length >= 2)
                return new VersionToken { Token = token.Substring(1, token.Length - 2), IsStrong = true };
            else
                throw new ArgumentException("Unable to parse token.", nameof(token));
        }
        /// <summary>
        /// Adds a middleware component to the IHttpRestRequestHandler that implements metadata for caching through HTTP headers.
        /// </summary>
        /// <param name="pipeline">The Rest HTTP pipeline.</param>
        /// <returns>A new IHttpRestRequestHandler containing the caching middleware.</returns>
        public static IHttpRestRequestHandler UseCaching(this IHttpRestRequestHandler pipeline)
            => pipeline.Use(next => async (context, req, conv) =>
            {
                RestResponse response;
                var httpReq = context.Request;
                if (httpReq.Headers.ContainsKey("If-None-Match"))
                {
                    var tokens = httpReq.Headers["If-None-Match"];
                    var versionTokens = new TokenMatching { Tokens = tokens.Select(ParseToken).ToList(), Matches = false };
                    response = await next(context, req.AddMetadata(versionTokens), conv);
                }
                else if (httpReq.Headers.ContainsKey("If-Match"))
                {
                    var tokens = httpReq.Headers["If-Match"];
                    var versionTokens = new TokenMatching { Tokens = tokens.Select(ParseToken).ToList(), Matches = true };
                    response = await next(context, req.AddMetadata(versionTokens), conv);
                }
                else
                    response = await next(context, req, conv);

                if (response.Metadata.TryGet<ResponseCaching>(out var cache))
                {
                    if (!cache.StoreAllowed)
                        context.Response.Headers["Cache-Control"] = "no-store";
                    else
                    {
                        var lst = new List<string>();
                        if (!cache.CacheAllowed)
                            lst.Add("no-cache");
                        if (cache.Validity > TimeSpan.Zero)
                        {
                            if (cache.CachePrivate)
                                lst.Add("private");
                            lst.Add($"max-age={(int)cache.Validity.TotalSeconds}");
                        }
                        context.Response.Headers["Cache-Control"] = string.Join(", ", lst);
                    }
                }
                response.Metadata.Execute<VersionToken>(vt => context.Response.Headers["ETag"] = vt.IsStrong ? "" : "W/" + string.Concat("\"", vt.Token, "\""));
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
                response.Metadata.Execute<CreatedResource>(loc =>
                {
                    var idProv = serviceProvider.GetRequiredService<IRestIdentityProvider>();
                    context.Response.Headers["Location"] = idProv.ToPath(loc.Address);
                });
                return response;
            });
    }
}
