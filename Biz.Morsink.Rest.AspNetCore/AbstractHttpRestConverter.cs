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
using Microsoft.Extensions.Options;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Microsoft.Extensions.DependencyInjection;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Abstract base class for IHttpRestConverter.
    /// </summary>
    public abstract class AbstractHttpRestConverter : IHttpRestConverter
    {
        private static readonly char[] EQUALS = new[] { '=' };
        private const string Curie = nameof(Curie);
        private const string Link = nameof(Link);
        private const string Location = nameof(Location);
        private const string SchemaLocation = "Schema-Location";

        public IRestIdentityProvider IdentityProvider { get; }

        private readonly IRestRequestScopeAccessor scopeAccessor;
        private readonly IOptions<RestAspNetCoreOptions> options;
        private readonly IEnumerable<IHttpContextManipulator> httpContextManipulators;

        /// <summary>
        /// Gets a boolean indicating if Curies are supported by this IHttpRestConverter.
        /// </summary>
        public virtual bool SupportsCuries => false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">A Rest IdentityProvider for path parsing and construction.</param>
        protected AbstractHttpRestConverter(IRestIdentityProvider identityProvider, IRestRequestScopeAccessor scopeAccessor, IOptions<RestAspNetCoreOptions> options, IEnumerable<IHttpContextManipulator> httpContextManipulators)
        {
            IdentityProvider = identityProvider;
            this.scopeAccessor = scopeAccessor;
            this.options = options;
            this.httpContextManipulators = httpContextManipulators;
        }
        /// <summary>
        /// Determines if the converter applies to the given HttpContext for interpretation of the request.
        /// </summary>
        /// <param name="context">The HttpContext associated with the HTTP Request.</param>
        /// <returns>A score ranging from 0 to 1.</returns>
        public abstract NegotiationScore AppliesToRequestScore(HttpContext context);
        /// <summary>
        /// Determines if the converter applies to the given HttpContext for serialization of the response.
        /// </summary>
        /// <param name="context">The HttpContext associated with the HTTP Request.</param>
        /// <param name="request">The Rest request as constructed by the Request converter.</param>
        /// <param name="response">The Rest response as returned by the Rest pipeline.</param>
        /// <returns>A score ranging from 0 to 1.</returns>
        public abstract NegotiationScore AppliesToResponseScore(HttpContext context, RestRequest request, RestResponse response);
        /// <summary>
        /// A converter is able to manipulate the RestRequest using this method.
        /// </summary>
        /// <param name="req">The RestRequest extracted from the HttpRequest.</param>
        /// <param name="context">The HttpContext for the request.</param>
        /// <returns>A optionally mutated RestRequest.</returns>
        public virtual RestRequest ManipulateRequest(RestRequest req, HttpContext context)
        {
            scopeAccessor.Scope.SetScopeItem(IdentityProvider.Prefixes.Copy());

            Lazy<byte[]> body = new Lazy<byte[]>(() =>
            {
                using (var ms = new MemoryStream())
                {
                    context.Request.Body.CopyTo(ms);
                    return ms.ToArray();
                }
            });
            if (SupportsCuries)
                ParseCurieHeaders(context.Request, scopeAccessor.Scope.GetScopeItem<RestPrefixContainer>());
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
            ManipulateHttpContext(response, context);

            await WriteResponse(response, context);
        }

        protected virtual async Task WriteResponse(RestResponse response, HttpContext context)
        {
            var rv = (response.UntypedResult as IHasRestValue)?.RestValue;

            if (rv != null)
            {
                ApplyHeaders(context.Response, response, rv,
                    SupportsCuries && options.Value.UseCuries
                    ? scopeAccessor.Scope.GetScopeItem<RestPrefixContainer>()
                    : null);

                await WriteValue(context.Response.Body, response, response.UntypedResult, rv);
            }
            else
            {
                await WriteResult(context.Response.Body, response, response.UntypedResult);
            }
        }


        /// <summary>
        /// Adds a 'Schema-Location' Http header with a link to the schema.
        /// </summary>
        /// <param name="response">The Http response.</param>
        /// <param name="rv">The Rest value.</param>
        protected void UseSchemaLocationHeader(HttpResponse response, IRestValue rv)
        {
            response.Headers[SchemaLocation] = new StringValues(IdentityProvider.ToPath(FreeIdentity<TypeDescriptor>.Create(rv.ValueType)));
        }
        /// <summary>
        /// Adds 'Link' HTTP headers for all the links in the Rest value.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="rv">The Rest value.</param>
        protected void UseLinkHeaders(HttpResponse response, IRestValue rv)
        {
            var links = rv.Links.Select(l => $"<{IdentityProvider.ToPath(l.Target)}>;rel={l.RelType}").ToList();
            response.Headers[Link] = new StringValues(links.ToArray());
        }
        /// <summary>
        /// Adds 'Curie' HTTP headers for all the curies for the current response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="prefixes">The Rest prefix container.</param>
        protected void UseCurieHeaders(HttpResponse response, RestPrefixContainer prefixes)
        {
            if (prefixes == null)
                return;

            response.Headers[Curie] = new StringValues(prefixes.Select(p => $"{p.Abbreviation}={p.Prefix}").ToArray());
        }
        protected void ParseCurieHeaders(HttpRequest request, RestPrefixContainer prefixes)
        {
            if (request.Headers.TryGetValue(Curie, out var curies))
            {
                foreach (var curie in curies)
                {
                    var parts = curie.Split(EQUALS, 2);
                    if (parts.Length == 2)
                        prefixes.Register(new RestPrefix(parts[1], parts[0]));
                }
            }
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
        /// Scores the Accept header based on some mimetype.
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="mimeType">A single mimetype the Rest converter accepts.</param>
        /// <returns>A score ranging from 0 to 1.</returns>
        protected NegotiationScore ScoreAcceptHeader(HttpRequest request, string mimeType, string suffix = null)
        {
            if (request.HttpContext.TryGetContextItem<AcceptStructure>(out var acceptStructure))
            {
                var (cas, q) = acceptStructure.Score(mimeType, suffix);
                return new NegotiationScore(cas?.MediaType ?? default, q);
            }
            else
                return HasAcceptHeader(request, mimeType) ? new NegotiationScore(mimeType, 1m) : new NegotiationScore(null, 0m);
        }

        /// <summary>
        /// Tries to get the Content-Type header.
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="mimeType">The contents of the Content-Type header.</param>
        /// <returns>True if the request has a Content-Type header, false otherwise.</returns>
        protected bool TryGetContentTypeHeader(HttpRequest request, out string mimeType)
        {
            var contentType = request.GetTypedHeaders().ContentType;
            if (contentType != null && contentType.Type.HasValue)
            {
                mimeType = contentType.Type.Value;
                return true;
            }
            else
            {
                mimeType = null;
                return false;
            }
        }
        protected NegotiationScore ScoreContentTypeAndAcceptHeaders(HttpRequest request, string mimeType, string suffix = null)
        {
            if (TryGetContentTypeHeader(request, out var mediaType))
                return mediaType == mimeType ? new NegotiationScore(mimeType, 1m) : new NegotiationScore(null, 0.1m);
            else
            {
                var score = ScoreAcceptHeader(request, mimeType, suffix);
                return score.WithQ(q => q / 2);
            }
        }

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
        /// <param name="prefixes">The Rest prefix container for this response.</param>
        protected abstract void ApplyHeaders(HttpResponse httpResponse, RestResponse response, IRestValue value, RestPrefixContainer prefixes);
        /// <summary>
        /// Override to write the body value to a Stream asynchronously.
        /// </summary>
        /// <param name="bodyStream">The Stream.</param>
        /// <param name="response">The response containing the value.</param>
        /// <param name="result">The result containing the value.</param>
        /// <param name="value">The Rest value.</param>
        /// <returns>A Task.</returns>
        protected abstract Task WriteValue(Stream bodyStream, RestResponse response, IRestResult result, IRestValue value);
        /// <summary>
        /// Override to write the body, when there is no value, to a Stream asynchronously.
        /// </summary>
        /// <param name="bodyStream">The Stream.</param>
        /// <param name="response">The response containing the value.</param>
        /// <param name="result">The result containing the value.</param>
        /// <returns>A Task.</returns>
        protected virtual Task WriteResult(Stream bodyStream, RestResponse response, IRestResult result)
            => Task.CompletedTask;

        protected virtual void ManipulateHttpContext(RestResponse response, HttpContext context)
        {
            ApplyGeneralHeaders(context.Response, response);

            if (response.UntypedResult is IRestFailure failure)
                switch (failure.Reason)
                {
                    case RestFailureReason.BadRequest:
                        context.Response.StatusCode = 400; break;
                    case RestFailureReason.NotFound:
                        switch (failure.FailureOn)
                        {
                            case RestEntityKind.Capability:
                                context.Response.StatusCode = 405; break;
                            case RestEntityKind.Repository:
                            case RestEntityKind.Resource:
                            default:
                                context.Response.StatusCode = 404; break;
                        }
                        break;
                    case RestFailureReason.Error:
                        context.Response.StatusCode = 500; break;
                    case RestFailureReason.NotExecuted:
                        context.Response.StatusCode = 412; break;
                }
            else if (response.UntypedResult is IRestRedirect redirect)
                switch (redirect.Type)
                {
                    case RestRedirectType.NotNecessary:
                        context.Response.StatusCode = 304; break;
                    case RestRedirectType.Permanent:
                        context.Response.Headers[Location] = IdentityProvider.ToPath(redirect.Target);
                        context.Response.StatusCode = 308;
                        break;
                    case RestRedirectType.Temporary:
                        context.Response.Headers[Location] = IdentityProvider.ToPath(redirect.Target);
                        context.Response.StatusCode = 307;
                        break;
                }

            if (response.IsSuccess && response.Metadata.TryGet<CreatedResource>(out var loc))
                context.Response.StatusCode = 201;

            if (response.UntypedResult.IsPending)
            {
                context.Response.StatusCode = 202;
                context.Response.Headers[Location] = IdentityProvider.ToPath(response.UntypedResult.AsPending().Job.Id);
            }
            foreach (var manipulator in httpContextManipulators)
                manipulator.ManipulateContext(context, response);
        }
        public virtual VersionMatcher DefaultVersionMatcher => VersionMatcher.Oldest;
    }
}
