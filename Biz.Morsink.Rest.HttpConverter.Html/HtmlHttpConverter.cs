using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// An Html Http converter
    /// </summary>
    public class HtmlHttpConverter : AbstractHttpRestConverter
    {
        private readonly IGeneralHtmlGenerator generator;
        private readonly IRestRequestScopeAccessor scopeAccessor;
        /// <summary>
        /// Constructor.
        /// </summary>
        public HtmlHttpConverter(IGeneralHtmlGenerator generator, IRestIdentityProvider provider, IRestRequestScopeAccessor scopeAccessor, IOptions<RestAspNetCoreOptions> restOptions)
            : base(provider, scopeAccessor, restOptions)
        {
            this.generator = generator;
            this.scopeAccessor = scopeAccessor;
        }
        public override decimal AppliesToRequestScore(HttpContext context)
            => ScoreContentTypeAndAcceptHeaders(context.Request, "text/html");
        public override decimal AppliesToResponseScore(HttpContext context, RestRequest request, RestResponse response)
            => ScoreAcceptHeader(context.Request, "text/html");

        public override object ParseBody(Type t, byte[] body)
        {
            throw new NotSupportedException();
        }

        protected override void ApplyGeneralHeaders(HttpResponse httpResponse, RestResponse response)
        {
            httpResponse.ContentType = "text/html";
        }

        protected override void ApplyHeaders(HttpResponse httpResponse, RestResponse response, IRestValue value, RestPrefixContainer prefixes)
        {
            UseLinkHeaders(httpResponse, value);
        }

        protected async Task WriteHtml(Stream bodyStream, string html)
        {
            var bytes = Encoding.UTF8.GetBytes(html ?? "");
            if (bytes.Length > 0)
                await bodyStream.WriteAsync(bytes, 0, bytes.Length);
        }

        protected override Task WriteValue(Stream bodyStream, RestResponse response, IRestResult result, IRestValue value)
            => WriteHtml(bodyStream, GenerateHtml(result));

        protected override Task WriteResult(Stream bodyStream, RestResponse response, IRestResult result)
            => WriteHtml(bodyStream, GenerateHtml(result));

        private string GenerateHtml(IRestResult result)
            => scopeAccessor.Scope
                .With(SerializationContext.Create(IdentityProvider))
                .Run(() => generator.GenerateHtml(result));

        public override VersionMatcher DefaultVersionMatcher => VersionMatcher.Newest;
    }
}
