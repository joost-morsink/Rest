using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    public abstract class RestForAspNetCore
    {
        public const int STATUS_NOTFOUND = 404;
        public const int STATUS_INTERNALSERVERERROR = 500;

        private readonly IRestRequestHandler handler;
        private readonly IHttpRestConverter[] converters;
        private readonly IRestIdentityProvider identityProvider;

        public RestForAspNetCore(IRestRequestHandler handler, IRestIdentityProvider identityProvider, IEnumerable<IHttpRestConverter> converters)
        {
            this.handler = handler;
            this.converters = converters.ToArray();
            this.identityProvider = identityProvider;

        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                var (req,conv) = ReadRequest(context);
                if (req == null)
                {
                    context.Response.StatusCode = STATUS_NOTFOUND;
                    await context.Response.WriteAsync("Cannot find resource");
                }
                else
                {
                    var resp = await handler.HandleRequest(req);
                    await WriteResponse(conv, context, resp);
                }
            }
            catch
            {
                context.Response.StatusCode = STATUS_INTERNALSERVERERROR;
                await context.Response.WriteAsync("An error occured.");
            }
        }
        public (RestRequest, IHttpRestConverter) ReadRequest(HttpContext context)
        {
            var request = context.Request;
            var req = RestRequest.Create(request.Method, identityProvider.Parse(request.Path),
                request.Query.SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<string, string>(kvp.Key, v))));
            for (int i = 0; i < converters.Length; i++)
                if (converters[i].Applies(context))
                    return (converters[i].ManipulateRequest(req, context), converters[i]);
            return (null,null);
        }
        public Task WriteResponse(IHttpRestConverter converter, HttpContext context, IRestResult result)
        {
            var response = converter.CreateResponse(result, context);
            return converter.SerializeResponse(response, context);
        }
    }
    public static class RestForAspNetCoreExt
    {
        public static IApplicationBuilder UseRestForAspNetCore(this IApplicationBuilder app)
            => app.UseMiddleware<RestForAspNetCore>();
    }
}
