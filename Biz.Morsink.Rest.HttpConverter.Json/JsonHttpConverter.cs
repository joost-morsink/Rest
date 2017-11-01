﻿using System;
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
            var accept = context.Request.Headers["Accept"].SelectMany(h => h.Split(',').Select(x => x.Trim())).ToArray();
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
            Lazy<byte[]> body = new Lazy<byte[]>(() =>
            {
                using (var ms = new MemoryStream())
                {
                    context.Request.Body.CopyTo(ms);
                    return ms.ToArray();
                }
            });
            return new RestRequest(req.Capability, req.Address, req.Parameters, ty => ParseBody(ty, body.Value), req.Metadata);
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
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            using (var jtr = new JsonTextReader(sr))
            {
                var ser = JsonSerializer.Create(options.Value.SerializerSettings);
                return ser.Deserialize(jtr, t);
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
                ManipulateHttpContext(response, context);

            if (context.Request.Method == "POST" && response.Metadata.TryGet<Location>(out var loc))
                context.Response.StatusCode = 201;

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

        private void ManipulateHttpContext(RestResponse response, HttpContext context)
        {
            if (response.UntypedResult is IRestFailure failure)
                switch (failure.Reason)
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
            else if (response.UntypedResult is IRestRedirect redirect)
                switch (redirect.Type)
                {
                    case RestRedirectType.NotNecessary:
                        context.Response.StatusCode = 304;
                        break;
                    case RestRedirectType.Permanent:
                        context.Response.Headers["Location"] = provider.ToPath(redirect.Target);
                        context.Response.StatusCode = 308;
                        break;
                    case RestRedirectType.Temporary:
                        context.Response.Headers["Location"] = provider.ToPath(redirect.Target);
                        context.Response.StatusCode = 307;
                        break;
                }
        }
    }
}
