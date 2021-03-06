﻿using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Constructor.
        /// </summary>
        public HtmlHttpConverter(IGeneralHtmlGenerator generator, IRestIdentityProvider provider, IRestRequestScopeAccessor scopeAccessor, IOptions<RestAspNetCoreOptions> restOptions, IEnumerable<IHttpContextManipulator> httpContextManipulators 
           )
            : base(provider, scopeAccessor, restOptions, httpContextManipulators)
        {
            this.generator = generator;
        }
        public override NegotiationScore AppliesToRequestScore(HttpContext context)
            => ScoreContentTypeAndAcceptHeaders(context.Request, "text/html");
        public override NegotiationScore AppliesToResponseScore(HttpContext context, RestRequest request, RestResponse response)
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
        /// <summary>
        /// Writes an html string to a body stream.
        /// </summary>
        /// <param name="bodyStream">The body stream to write to.</param>
        /// <param name="html">The html string to write.</param>
        /// <returns>An asynchronous result.</returns>
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
            => generator.GenerateHtml(result);

        public override VersionMatcher DefaultVersionMatcher => VersionMatcher.Newest;
    }
}
