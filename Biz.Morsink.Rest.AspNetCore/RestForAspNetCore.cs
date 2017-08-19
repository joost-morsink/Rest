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
                var req = ReadRequest(context);
                if (req == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Cannot find resource");
                }
                else
                {
                    var resp = await handler.HandleRequest(req);
                    await WriteResponse(context.Response, resp);
                }
            }
            catch
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An error occured.");
            }
        }
        public RestRequest ReadRequest(HttpContext context)
        {
            var request = context.Request;
            var req = RestRequest.Create(request.Method, identityProvider.Parse(request.Path),
                request.Query.SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<string, string>(kvp.Key, v))));
            for (int i = 0; i < converters.Length; i++)
                if (converters[i].Applies(context))
                    return converters[i].ManipulateRequest(req, context);
            return null;
        }
        public abstract Task WriteResponse(HttpResponse response, RestResponse rest);
    }
    public static class RestForAspNetCoreExt
    {
        public static IApplicationBuilder UseRestForAspNetCore(this IApplicationBuilder app)
            => app.UseMiddleware<RestForAspNetCore>();
    }
}
