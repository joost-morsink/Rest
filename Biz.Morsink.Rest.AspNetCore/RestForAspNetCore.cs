using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next request delegate in the pipeline.</param>
        /// <param name="restHandler">A Rest request handler.</param>
        /// <param name="httpHandler">A Rest HTTP pipeline.</param>
        /// <param name="identityProvider">A Rest identity provider.</param>
        /// <param name="converters">A collection of applicable Rest converters for HTTP.</param>
        public RestForAspNetCore(RequestDelegate next, IRestRequestHandler restHandler, IHttpRestRequestHandler httpHandler, IRestIdentityProvider identityProvider, IEnumerable<IHttpRestConverter> converters, IAuthorizationProvider authorizationProvider)
        {
            this.handler = restHandler;
            this.converters = converters.ToArray();
            this.identityProvider = identityProvider;
            this.restRequestDelegate = httpHandler.GetRequestDelegate(restHandler);
            this.authorizationProvider = authorizationProvider;
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
                context.Items[nameof(IHttpRestConverter)] = conv;

                if (req == null)
                {
                    context.Response.StatusCode = STATUS_NOTFOUND;
                    await context.Response.WriteAsync("Cannot find resource");
                }
                else
                {
                    if(authorizationProvider.IsAllowed(context.User, req.Address, req.Capability))
                    {
                        var resp = await restRequestDelegate(context, req, conv);
                        await WriteResponse(conv, context, resp);
                    }
                    else
                    {
                        // TODO: The following status assignment is very simplistic and should be refactored at a later stage.
                        context.Response.StatusCode = context.User.Identity.IsAuthenticated ? STATUS_FORBIDDEN : STATUS_NOTAUTHENTICATED;
                    }
                }
            }
            catch (UnsupportedMediaTypeException)
            {
                context.Response.StatusCode = STATUS_UNSUPPORTED_MEDIA_TYPE;
            }
            catch(Exception)
            {
                context.Response.StatusCode = STATUS_INTERNALSERVERERROR;
                await context.Response.WriteAsync("An error occured.");
            }
        }
        private (RestRequest, IHttpRestConverter) ReadRequest(HttpContext context)
        {
            var request = context.Request;
            var req = RestRequest.Create(request.Method, identityProvider.Parse(request.Path + request.QueryString),
                request.Query.SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<string, string>(kvp.Key, v))));
            for (int i = 0; i < converters.Length; i++)
                if (converters[i].Applies(context))
                    return (converters[i].ManipulateRequest(req, context), converters[i]);
            return (null, null);
        }
        private Task WriteResponse(IHttpRestConverter converter, HttpContext context, RestResponse response)
        {
            return converter.SerializeResponse(response, context);
        }
    }
  
}
