using System;
using Biz.Morsink.Rest.AspNetCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    public class JsonHttpConverter : IHttpRestConverter
    {
        private readonly IOptions<JsonHttpConverterOptions> options;
        private readonly IRestIdentityProvider provider;

        public JsonHttpConverter(IOptions<JsonHttpConverterOptions> options, IRestIdentityProvider provider)
        {
            this.options = options;
            this.provider = provider;
        }
        public bool Applies(HttpContext context)
        {
            var accept = context.Request.Headers["Accept"].ToArray();
            return accept.Contains("application/json");
        }

        public RestRequest ManipulateRequest(RestRequest req, HttpContext context)
        {
            return req;
        }

        public object ParseBody(Type t, byte[] body)
        {
            using (var ms = new MemoryStream(body))
            {
                var ser = JsonSerializer.Create(options.Value.SerializerSettings);
                return ser.Deserialize(new JsonTextReader(new StreamReader(ms, Encoding.UTF8)), t);
            }
        }

        public async Task SerializeResponse(RestResponse response, HttpContext context)
        {
            context.Response.Headers["Content-Type"] = "application/json";
            var ser = JsonSerializer.Create(options.Value.SerializerSettings);
            var rv = (response.UntypedResult as IHasRestValue)?.RestValue;
            if (rv != null)
            {
                if (rv.Links.Count > 0)
                {
                    foreach (var x in rv.Links.GroupBy(l => l.RelType))
                        context.Response.Headers[$"Link-{x.Key}"] = new StringValues(x.Select(l => provider.ToPath(l.Target)).ToArray());
                }
                var json = JObject.FromObject(rv.Value, ser);
                var sb = new StringBuilder();
                {
                    json.WriteTo(new JsonTextWriter(new StringWriter(sb)));
                    var body = sb.ToString();
                    await context.Response.WriteAsync(body);
                    //await json.WriteToAsync(new JsonTextWriter(new StreamWriter(context.Response.Body)));
                }
            }
        }
    }
}
