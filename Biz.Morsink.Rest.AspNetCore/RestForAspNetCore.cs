﻿using Biz.Morsink.Identity.PathProvider;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.MediaTypes;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// An ASP.Net Core middleware component that wraps all the necessary components for handling Restful requests.
    /// </summary>
    public class RestForAspNetCore
    {
        /// <summary>
        /// Value for the not authenticated HTTP status.
        /// </summary>
        public const int STATUS_NOTAUTHENTICATED = 401;
        /// <summary>
        /// Value for the forbidden HTTP status.
        /// </summary>
        public const int STATUS_FORBIDDEN = 403;
        /// <summary>
        /// Value for the not found HTTP status.
        /// </summary>
        public const int STATUS_NOTFOUND = 404;
        /// <summary>
        /// Value for the not acceptable HTTP status.
        /// </summary>
        public const int STATUS_NOTACCEPTABLE = 406;
        /// <summary>
        /// Value for the internal server error HTTP status.
        /// </summary>
        public const int STATUS_INTERNALSERVERERROR = 500;
        /// <summary>
        /// Value for the unsupported media type HTTP status.
        /// </summary>
        public const int STATUS_UNSUPPORTED_MEDIA_TYPE = 415;

        private readonly IRestRequestHandler handler;
        private readonly IHttpRestConverter[] converters;
        private readonly IRestIdentityProvider identityProvider;
        private readonly RestRequestDelegate restRequestDelegate;
        private readonly IAuthorizationProvider authorizationProvider;
        private readonly IEnumerable<IRestExceptionListener> exceptionListeners;
        private readonly IMediaTypeProvider mediaTypeProvider;
        private readonly IOptions<RestAspNetCoreOptions> options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next request delegate in the pipeline.</param>
        /// <param name="restHandler">A Rest request handler.</param>
        /// <param name="httpHandler">A Rest HTTP pipeline.</param>
        /// <param name="identityProvider">A Rest identity provider.</param>
        /// <param name="converters">A collection of applicable Rest converters for HTTP.</param>
        public RestForAspNetCore(RequestDelegate next, IRestRequestHandler restHandler, IHttpRestRequestHandler httpHandler, IRestIdentityProvider identityProvider, IEnumerable<IHttpRestConverter> converters, IAuthorizationProvider authorizationProvider, IEnumerable<IRestExceptionListener> exceptionListeners, IMediaTypeProvider mediaTypeProvider, IOptions<RestAspNetCoreOptions> options)
        {
            this.handler = restHandler;
            this.converters = converters.ToArray();
            this.identityProvider = identityProvider;
            this.restRequestDelegate = httpHandler.GetRequestDelegate(restHandler);
            this.authorizationProvider = authorizationProvider;
            this.exceptionListeners = exceptionListeners;
            this.mediaTypeProvider = mediaTypeProvider;
            this.options = options;
        }
        /// <summary>
        /// This method implements the RequestDelegate for the Rest middleware component.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                var (req, conv) = ReadRequest(context);
                context.SetContextItem(conv);
                IHttpRestConverter responseConv = null;
                if (req == null)
                {
                    context.Response.StatusCode = STATUS_NOTACCEPTABLE;
                    await context.Response.WriteAsync("Not acceptable");
                }
                else
                {
                    if (authorizationProvider.IsAllowed(context.User, req.Address, req.Capability))
                    {
                        var resp = await restRequestDelegate(context, req, conv);
                        if (options.Value.VersionHeader != null && resp.Metadata.TryGet(out Versioning ver))
                        {
                            var supportedHeader = options.Value.SupportedVersionsHeader ?? "Supported-Versions";
                            if (ver.Current != null)
                                context.Response.Headers[options.Value.VersionHeader] = ver.Current.ToString();
                            context.Response.Headers[supportedHeader] = new StringValues(ver.Supported.Select(v => v.ToString()).ToArray());
                        }

                        responseConv = GetResponseConverter(context, req, resp);
                        await WriteResponse(responseConv, context, resp);
                    }
                    else
                    {
                        // TODO: The following status assignment is very simplistic and should be refactored at a later stage.
                        context.Response.StatusCode = context.User.Identity.IsAuthenticated ? STATUS_FORBIDDEN : STATUS_NOTAUTHENTICATED;
                    }
                }
            }
            catch (HttpException hex)
            {
                context.Response.StatusCode = hex.StatusCode;
            }
            catch (Exception ex)
            {
                foreach (var listener in exceptionListeners)
                    listener.UnexpectedExceptionOccured(ex);
                context.Response.StatusCode = STATUS_INTERNALSERVERERROR;
                await context.Response.WriteAsync("An error occured.");
            }
        }
        private (RestRequest, IHttpRestConverter) ReadRequest(HttpContext context)
        {
            var request = context.Request;

            if (context.Request.Headers.TryGetValue("Accept", out var acceptHeaders))
            {
                var accStruct = new AcceptStructure(acceptHeaders.SelectMany(h => h.Split(',')).ToList());
                context.SetContextItem(accStruct);
            }
            else
                return (null, null);

            var (best, bestQ) = DetermineBestConverter(context);
            if (best == null)
                return (null, null);

            context.SetContextItem(bestQ);
            RestRequest req = GetRequest(context, request, best, bestQ);
            return (best?.ManipulateRequest(req, context), best);
        }

        private (IHttpRestConverter, NegotiationScore) DetermineBestConverter(HttpContext context)
        {
            IHttpRestConverter best = null;
            NegotiationScore bestQ = default;
            for (int i = 0; i < converters.Length; i++)
            {
                var q = converters[i].AppliesToRequestScore(context);
                if (q.Q > bestQ.Q)
                {
                    bestQ = q;
                    best = converters[i];
                }
            }
            return (best, bestQ);
        }

        private RestRequest GetRequest(HttpContext context, HttpRequest request, IHttpRestConverter best, NegotiationScore bestQ)
        {
            var vm = GetVersionMatcher(context) ?? best.DefaultVersionMatcher;

            var requestedType = mediaTypeProvider.GetTypeForMediaType(MediaType.Parse(bestQ.MediaType).WithoutSuffix());
            if (requestedType != null)
            {
                var ver = identityProvider.GetSupportedVersions(requestedType).Where(t => t.Item2 == requestedType).Select(t => t.Item1).First();
                //var versioning = new Versioning(() => new VersionInRange(ver, new[] { ver }));
                return RestRequest.Create(request.Method, identityProvider.Parse(request.Path + request.QueryString, versionMatcher: VersionMatcher.OnMajor(ver.Major)),
                    request.Query.SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<string, string>(kvp.Key, v))));

            }
            else
            {
                var req = RestRequest.Create(request.Method, identityProvider.Parse(request.Path + request.QueryString, versionMatcher: vm),
                    request.Query.SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<string, string>(kvp.Key, v))));

                var matches = identityProvider.Match(request.Path + request.QueryString);
                var match = vm.Match(matches.Select(m => (m.Version, m)));
                var versioning = new Versioning(() => new VersionInRange(match.Item1, matches.Select(m => m.Version)));

                return req.AddMetadata(versioning);
            }
        }

        private VersionMatcher? GetVersionMatcher(HttpContext context)
        {
            if (options.Value.VersionHeader != null && context.Request.Headers.TryGetValue(options.Value.VersionHeader, out var versHdr))
            {
                var requestedVersion = versHdr.First().ToLower();
                switch (requestedVersion)
                {
                    case "latest":
                    case "newest":
                        return VersionMatcher.Newest;
                    case "oldest":
                        return VersionMatcher.Oldest;
                    default:
                        var idx = requestedVersion.IndexOf(".");
                        if (idx > 0)
                            requestedVersion = requestedVersion.Substring(0, idx);
                        if (int.TryParse(requestedVersion, out var x))
                            return VersionMatcher.OnMajor(x);
                        else
                            return null;
                }
            }
            else
                return null;
        }

        private IHttpRestConverter GetResponseConverter(HttpContext context, RestRequest request, RestResponse response)
        {
            IHttpRestConverter best = null;
            NegotiationScore bestQ = default;
            for (int i = 0; i < converters.Length; i++)
            {
                var q = converters[i].AppliesToResponseScore(context, request, response);
                if (q.Q > bestQ.Q)
                {
                    bestQ = q;
                    best = converters[i];
                }
            }
            return best;
        }
        private Task WriteResponse(IHttpRestConverter converter, HttpContext context, RestResponse response)
        {
            return converter.SerializeResponse(response, context);
        }
    }
}
