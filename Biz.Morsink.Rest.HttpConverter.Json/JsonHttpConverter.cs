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
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Component that converts Http Json bodies from and to Rest requests and responses.
    /// </summary>
    public class JsonHttpConverter : IHttpRestConverter
    {
        private readonly IOptions<JsonHttpConverterOptions> options;
        private readonly IRestIdentityProvider provider;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Configuration for the component.</param>
        /// <param name="provider">A Rest IdentityProvider for path parsing and construction.</param>
        public JsonHttpConverter(IOptions<JsonHttpConverterOptions> options, IRestIdentityProvider provider)
        {
            this.options = options;
            this.provider = provider;
        }
        /// <summary>
        /// Determines if the converter applies to the given HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext associated with the Http Request.</param>
        /// <returns>True if this converter is applicable to the context.</returns>
        public bool Applies(HttpContext context)
        {
            var accept = context.Request.Headers["Accept"].ToArray();
            return accept.Contains("application/json");
        }
        /// <summary>
        /// The JsonHttpConverter does not manipulate requests.
        /// </summary>
        /// <param name="req">The RestRequest extracted from the HttpRequest.</param>
        /// <param name="context">The HttpContext for the request. Not used.</param>
        /// <returns>The req parameter.</returns>
        public RestRequest ManipulateRequest(RestRequest req, HttpContext context)
        {
            return req;
        }
        /// <summary>
        /// Json parser.
        /// </summary>
        /// <param name="t">The type of Json present in the body.</param>
        /// <param name="body">The raw data for the Json body.</param>
        /// <returns>A parsed object of the specified type.</returns>
        public object ParseBody(Type t, byte[] body)
        {
            using (var ms = new MemoryStream(body))
            {
                var ser = JsonSerializer.Create(options.Value.SerializerSettings);
                return ser.Deserialize(new JsonTextReader(new StreamReader(ms, Encoding.UTF8)), t);
            }
        }
        /// <summary>
        /// Asynchronously serializes a RestResponse to a Json document on the response stream.
        /// </summary>
        /// <param name="response">The response to serialize.</param>
        /// <param name="context">The HttpContext for the request.</param>
        /// <returns>A Task describing the asynchronous progress of the serialization.</returns>
        public async Task SerializeResponse(RestResponse response, HttpContext context)
        {
            context.Response.Headers["Content-Type"] = "application/json";
            if (!response.IsSuccess)
                setFailureStatusCode(response, context);

            var ser = JsonSerializer.Create(options.Value.SerializerSettings);
            var rv = (response.UntypedResult as IHasRestValue)?.RestValue;
            if (rv != null)
            {
                context.Response.Headers["Schema-Location"] = new StringValues(provider.ToPath(FreeIdentity<TypeDescriptor>.Create(rv.ValueType)));

                var links = rv.Links.Select(l => $"<{provider.ToPath(l.Target)}>;rel={l.RelType}").ToList();
                links.Add($"<{provider.ToPath(FreeIdentity<TypeDescriptor>.Create(rv.ValueType))}>;rel=describedby");
                context.Response.Headers["Link"] = new StringValues(links.ToArray());

                var json = JToken.FromObject(rv.Value, ser);
                var sb = new StringBuilder();
                {
                    json.WriteTo(new JsonTextWriter(new StringWriter(sb)));
                    var body = sb.ToString();
                    await context.Response.WriteAsync(body);
                    //await json.WriteToAsync(new JsonTextWriter(new StreamWriter(context.Response.Body)));
                }
            }
        }

        private static void setFailureStatusCode(RestResponse response, HttpContext context)
        {
            switch (response.UntypedResult.AsFailure().Reason)
            {
                case RestFailureReason.BadRequest:
                    context.Response.StatusCode = 400;
                    break;
                case RestFailureReason.NotFound:
                    context.Response.StatusCode = 404;
                    break;
                case RestFailureReason.Error:
                    context.Response.StatusCode = 500;
                    break;
            }
        }
    }
}
