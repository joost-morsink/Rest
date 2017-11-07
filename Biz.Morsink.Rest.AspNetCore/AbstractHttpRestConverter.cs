using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Biz.Morsink.Rest.Metadata;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Abstract base class for IHttpRestConverter.
    /// </summary>
    public abstract class AbstractHttpRestConverter : IHttpRestConverter
    {
        public IRestIdentityProvider IdentityProvider { get; }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">A Rest IdentityProvider for path parsing and construction.</param>
        protected AbstractHttpRestConverter(IRestIdentityProvider identityProvider)
        {
            IdentityProvider = identityProvider;
        }
        /// <summary>
        /// Determines if the converter applies to the given HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext associated with the HTTP Request.</param>
        /// <returns>True if this converter is applicable to the context.</returns>
        public abstract bool Applies(HttpContext context);
        /// <summary>
        /// A converter is able to manipulate the RestRequest using this method.
        /// </summary>
        /// <param name="req">The RestRequest extracted from the HttpRequest.</param>
        /// <param name="context">The HttpContext for the request.</param>
        /// <returns>A optionally mutated RestRequest.</returns>
        public virtual RestRequest ManipulateRequest(RestRequest req, HttpContext context)
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
        /// A converter is able to parse an HTTP body if it knows the type of data to expect. 
        /// </summary>
        /// <param name="t">The expected type of the body.</param>
        /// <param name="body">A byte array containing the raw HTTP body.</param>
        /// <returns>A parsed object.</returns>
        public abstract object ParseBody(Type t, byte[] body);
        /// <summary>
        /// Asynchronously serializes a RestResponse to a document on the response stream.
        /// </summary>
        /// <param name="response">The response to serialize.</param>
        /// <param name="context">The HttpContext for the request.</param>
        /// <returns>A Task describing the asynchronous progress of the serialization.</returns>
        public virtual async Task SerializeResponse(RestResponse response, HttpContext context)
        {
            ApplyGeneralHeaders(context.Response, response);
            if (!response.IsSuccess)
                ManipulateHttpContext(response, context);

            if (response.IsSuccess && response.Metadata.TryGet<CreatedResource>(out var loc))
                context.Response.StatusCode = 201;

            if (response.UntypedResult.IsPending)
            {
                context.Response.StatusCode = 202;
                context.Response.Headers["Location"] = IdentityProvider.ToPath(response.UntypedResult.AsPending().Job.Id);
            }

            var rv = (response.UntypedResult as IHasRestValue)?.RestValue;
            if (rv != null)
            {
                ApplyHeaders(context.Response, response, rv);
                await WriteValue(context.Response.Body, rv);
            }
        }
        /// <summary>
        /// Adds a 'Schema-Location' Http header with a link to the schema.
        /// </summary>
        /// <param name="response">The Http response.</param>
        /// <param name="rv">The Rest value.</param>
        protected void UseSchemaLocationHeader(HttpResponse response, IRestValue rv)
        {
            response.Headers["Schema-Location"] = new StringValues(IdentityProvider.ToPath(FreeIdentity<TypeDescriptor>.Create(rv.ValueType)));
        }
        /// <summary>
        /// Adds 'Link' HTTP headers for all the links in the Rest value.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="rv">The Rest value.</param>
        /// <param name="includeSchema">Should be true if the schema location is to be added as a 'Link' with Reltype 'describedby'.</param>
        protected void UseLinkHeaders(HttpResponse response, IRestValue rv, bool includeSchema = true)
        {
            var links = rv.Links.Select(l => $"<{IdentityProvider.ToPath(l.Target)}>;rel={l.RelType}").ToList();
            if (includeSchema)
                links.Add($"<{IdentityProvider.ToPath(FreeIdentity<TypeDescriptor>.Create(rv.ValueType))}>;rel=describedby");
            response.Headers["Link"] = new StringValues(links.ToArray());
        }
        /// <summary>
        /// Determines if the request has a specified header (optionally with a specfied value)
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="header">The header name.</param>
        /// <param name="value">Optionally a header value.</param>
        /// <returns>True if the header is found.</returns>
        protected bool HasHeader(HttpRequest request, string header, string value = null)
            => request.Headers.TryGetValue(header, out var values)
                && (value == null || values.SelectMany(h => h.Split(',').Select(x => x.Trim())).Contains(value));
        /// <summary>
        /// Determines if the request has an Accept header with a specfied value.
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="acceptValue">The Accept value to search for.</param>
        /// <returns>True if the Accept header is found with the specified value.</returns>
        protected bool HasAcceptHeader(HttpRequest request, string acceptValue)
            => HasHeader(request, "Accept", acceptValue);
        /// <summary>
        /// Override to apply general HTTP headers (like Content-Type).
        /// </summary>
        /// <param name="httpResponse">The HTTP response to apply the headers to.</param>
        /// <param name="response">The Rest response.</param>
        protected abstract void ApplyGeneralHeaders(HttpResponse httpResponse, RestResponse response);
        /// <summary>
        /// Override to apply specific HTTP headers.
        /// </summary>
        /// <param name="httpResponse">The HTTP response to apply the headers to.</param>
        /// <param name="response">The Rest response.</param>
        /// <param name="value">The Rest response's underlying Rest value.</param>
        protected abstract void ApplyHeaders(HttpResponse httpResponse, RestResponse response, IRestValue value);
        /// <summary>
        /// Override to write the body value to a Stream asynchronously.
        /// </summary>
        /// <param name="bodyStream">The Stream.</param>
        /// <param name="value">The Rest value.</param>
        /// <returns>A Task.</returns>
        protected abstract Task WriteValue(Stream bodyStream, IRestValue value);

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
                    case RestFailureReason.NotExecuted:
                        context.Response.StatusCode = 412;
                        break;
                }
            else if (response.UntypedResult is IRestRedirect redirect)
                switch (redirect.Type)
                {
                    case RestRedirectType.NotNecessary:
                        context.Response.StatusCode = 304;
                        break;
                    case RestRedirectType.Permanent:
                        context.Response.Headers["Location"] = IdentityProvider.ToPath(redirect.Target);
                        context.Response.StatusCode = 308;
                        break;
                    case RestRedirectType.Temporary:
                        context.Response.Headers["Location"] = IdentityProvider.ToPath(redirect.Target);
                        context.Response.StatusCode = 307;
                        break;
                }
        }

    }
}
