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
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Component that converts HTTP Json bodies from and to Rest requests and responses.
    /// </summary>
    public class JsonHttpConverter : AbstractHttpRestConverter
    {
        public const string MEDIA_TYPE = "application/json";
        private readonly IOptions<JsonHttpConverterOptions> options;
        private readonly IRestRequestScopeAccessor restRequestScopeAccessor;
        private readonly JsonRestSerializer restSerializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">Configuration for the component.</param>
        /// <param name="provider">A Rest IdentityProvider for path parsing and construction.</param>
        public JsonHttpConverter(IOptions<JsonHttpConverterOptions> options, IRestRequestScopeAccessor restRequestScopeAccessor, IRestIdentityProvider provider, IOptions<RestAspNetCoreOptions> restOptions, JsonRestSerializer restSerializer)
            : base(provider, restRequestScopeAccessor, restOptions)
        {
            this.options = options;
            this.restRequestScopeAccessor = restRequestScopeAccessor;
            this.restSerializer = restSerializer;
        }
        /// <summary>
        /// Determines if the converter applies to the given HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext associated with the HTTP Request.</param>
        /// <returns>A score ranging from 0 to 1.</returns>
        public override decimal AppliesToRequestScore(HttpContext context)
            => ScoreContentTypeAndAcceptHeaders(context.Request, MEDIA_TYPE);
        /// <summary>
        /// Determines if the converter applies to the given context.
        /// </summary>
        /// <param name="context">The HttpContext associated with the HTTP Request.</param>
        /// <param name="request">The Rest request as constructed by the Request converter.</param>
        /// <param name="response">The Rest response as returned by the Rest pipeline.</param>
        /// <returns>A score ranging from 0 to 1.</returns>
        public override decimal AppliesToResponseScore(HttpContext context, RestRequest request, RestResponse response)
            => ScoreAcceptHeader(context.Request, MEDIA_TYPE);
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
                try
                {
                    return restSerializer.ReadJson(jtr, t);
                }
                catch (RestFailureException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new RestFailureException(RestResult.BadRequest<object>($"Parse error: {e.Message}"), e.Message, e);
                }
            }
        }

        protected override void ApplyGeneralHeaders(HttpResponse httpResponse, RestResponse response)
        {
            httpResponse.ContentType = MEDIA_TYPE;
        }
        protected override void ApplyHeaders(HttpResponse httpResponse, RestResponse response, IRestValue value, RestPrefixContainer prefixes)
        {
            UseSchemaLocationHeader(httpResponse, value);
            if (options.Value.LinkLocation == null)
                UseLinkHeaders(httpResponse, value);
            UseCurieHeaders(httpResponse, prefixes);
        }
        protected override async Task WriteValue(Stream bodyStream, RestResponse response, IRestResult result, IRestValue value)
        {
            using (var ms = new MemoryStream())
            {
                var context = Serialization.SerializationContext.Create(IdentityProvider);
                using (var swri = new StreamWriter(ms))
                using (var wri = new JsonTextWriter(swri))
                    restSerializer.WriteJson(wri, value.Value);
                var body = ms.ToArray();
                await bodyStream.WriteAsync(body, 0, body.Length);
            }
        }

        public override bool SupportsCuries => true;
    }
}
