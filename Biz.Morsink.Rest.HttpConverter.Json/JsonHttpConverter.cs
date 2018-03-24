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
using Biz.Morsink.Rest.Metadata;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Component that converts HTTP Json bodies from and to Rest requests and responses.
    /// </summary>
    public class JsonHttpConverter : AbstractHttpRestConverter
    {
        private readonly IOptions<JsonHttpConverterOptions> options;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">Configuration for the component.</param>
        /// <param name="provider">A Rest IdentityProvider for path parsing and construction.</param>
        public JsonHttpConverter(IOptions<JsonHttpConverterOptions> options, IRestIdentityProvider provider) : base(provider)
        {
            this.options = options;
        }
        /// <summary>
        /// Determines if the converter applies to the given HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext associated with the HTTP Request.</param>
        /// <returns>True if this converter is applicable to the context.</returns>
        public override bool Applies(HttpContext context)
            => HasAcceptHeader(context.Request, "application/json");
        /// <summary>
        /// Json parser.
        /// </summary>
        /// <param name="t">The type of Json present in the body.</param>
        /// <param name="body">The raw data for the Json body.</param>
        /// <returns>A parsed object of the specified type.</returns>
        public override object ParseBody(Type t, byte[] body)
        {
            using (var ms = new MemoryStream(body))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            using (var jtr = new JsonTextReader(sr))
            {
                var ser = JsonSerializer.Create(options.Value.SerializerSettings);
                return ser.Deserialize(jtr, t);
            }
        }

        protected override void ApplyGeneralHeaders(HttpResponse httpResponse, RestResponse response)
        {
            httpResponse.ContentType = "application/json";
        }
        protected override void ApplyHeaders(HttpResponse httpResponse, RestResponse response, IRestValue value)
        {
            UseSchemaLocationHeader(httpResponse, value);
            UseLinkHeaders(httpResponse, value);
        }
        protected override async Task WriteValue(Stream bodyStream, IRestValue value)
        {
            var ser = JsonSerializer.Create(options.Value.SerializerSettings);

            using (var ms = new MemoryStream())
            {
                using (var swri = new StreamWriter(ms))
                using (var wri = new JsonTextWriter(swri))
                    ser.Serialize(wri, value.Value);

                var body = ms.ToArray();
                await bodyStream.WriteAsync(body, 0, body.Length);
            }
        }
        public override bool SupportsCuries => true;
    }
}
