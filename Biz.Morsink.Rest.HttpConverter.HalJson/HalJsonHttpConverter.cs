using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    /// <summary>
    /// This class is an Http converter implementation for the Hal Json media type.
    /// </summary>
    public class HalJsonHttpConverter : AbstractHttpRestConverter
    {
        /// <summary>
        /// The media type for Hal Json: application/hal+json
        /// </summary>
        public const string MEDIA_TYPE = "application/hal+json";

        private readonly HalJsonRestSerializer serializer;
        private readonly IOptions<HalJsonConverterOptions> options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">Options for the Http converter.</param>
        /// <param name="identityProvider">A Rest identity provider.</param>
        /// <param name="serializer">The HalSerializer instance to use.</param>
        /// <param name="restOptions">Options for the Rest for ASP.Net core general component.</param>
        public HalJsonHttpConverter(IRestIdentityProvider identityProvider, IRestRequestScopeAccessor scopeAccessor, IOptions<RestAspNetCoreOptions> restOptions, IEnumerable<IHttpContextManipulator> httpContextManipulators, HalJsonRestSerializer serializer, IOptions<HalJsonConverterOptions> options)
            : base(identityProvider, scopeAccessor, restOptions, httpContextManipulators)
        {
            this.serializer = serializer;
            this.options = options;
        }
        /// <summary>
        /// Checks the Accept header for the Hal Json media type.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <returns>A score indicating how much this converter applies.</returns>
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
        /// Parses a Json body.
        /// </summary>
        /// <param name="t">The destination type.</param>
        /// <param name="body">The body.</param>
        /// <returns>A deserialized object of the specified type.</returns>
        public override object ParseBody(Type t, byte[] body)
        {
            using (var ms = new MemoryStream(body))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            using (var jtr = new JsonTextReader(sr))
            {
                var ser = JsonSerializer.Create(options.Value.SerializerSettings);
                try
                {
                    return serializer.ReadJson(jtr, t);
                }
                catch (Exception e)
                {
                    throw new RestFailureException(RestResult.BadRequest<object>($"Parse error: {e.Message}"), e.Message, e);
                }
            }
        }
        /// <summary>
        /// The only general header that is applied is the Content-Type header, which is set to the Hal Json media type.
        /// </summary>
        /// <param name="httpResponse">The HTTP response to apply the headers to.</param>
        /// <param name="response">The Rest response.</param>
        protected override void ApplyGeneralHeaders(HttpResponse httpResponse, RestResponse response)
        {
            httpResponse.ContentType = MEDIA_TYPE;
        }
        /// <summary>
        /// No headers apply specifically to Hal Json values.
        /// </summary>
        /// <param name="httpResponse">The HTTP response to apply the headers to.</param>
        /// <param name="response">The Rest response.</param>
        /// <param name="value">The Rest response's underlying Rest value.</param>
        /// <param name="prefixes">The Rest prefix container for this response.</param>
        protected override void ApplyHeaders(HttpResponse httpResponse, RestResponse response, IRestValue value, RestPrefixContainer prefixes)
        {
        }
        /// <summary>
        /// Writes a Hal Json response.
        /// </summary>
        /// <param name="bodyStream">The Stream.</param>
        /// <param name="response">The response containing the value.</param>
        /// <param name="result">The result containing the value.</param>
        /// <param name="value">The Rest value.</param>
        /// <returns>A Task.</returns>
        protected override async Task WriteValue(Stream bodyStream, RestResponse response, IRestResult result, IRestValue value)
        {
            using (var ms = new MemoryStream())
            {
                var context = Serialization.SerializationContext.Create(IdentityProvider);
                using (var swri = new StreamWriter(ms))
                using (var wri = new JsonTextWriter(swri))
                    serializer.WriteJson(wri, value);
                var body = ms.ToArray();
                await bodyStream.WriteAsync(body, 0, body.Length);
            }
        }
        /// <summary>
        /// Hal Json does not support Curies for links.
        /// </summary>
        public override bool SupportsCuries => false;
    }
}
