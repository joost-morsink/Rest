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

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    public class JsonHttpConverter : IHttpRestConverter
    {
        private readonly IOptions<JsonHttpConverterOptions> options;

        public JsonHttpConverter(IOptions<JsonHttpConverterOptions> options)
        {
            this.options = options;
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
            var rv = (response as IHasRestValue)?.RestValue;
            if (rv != null)
            {
                var json = JObject.FromObject(rv, ser);
                await json.WriteToAsync(new JsonTextWriter(new StreamWriter(context.Response.Body)));
            }
        }
    }
}
