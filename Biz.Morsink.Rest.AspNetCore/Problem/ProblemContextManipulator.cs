using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biz.Morsink.Rest.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Biz.Morsink.Rest.AspNetCore.Problem
{
    /// <summary>
    /// An IHttpContextManipulator to set the Content-Type to a problem media type if the object being serialized is a Problem (RFC7807) type.
    /// </summary>
    public class ProblemContextManipulator : IHttpContextManipulator
    {
        /// <summary>
        /// Creates a ProblemContextManipulator for the application/problem+json media type.
        /// </summary>
        /// <param name="typeRepresentations">The type representations that may transform an object into a Problem.</param>
        /// <returns>A ProblemContextManipulator instance for the application/problem+json media type.</returns>
        public static ProblemContextManipulator Json(ITypeRepresentations typeRepresentations) => new ProblemContextManipulator(typeRepresentations, "application/json", "application/problem+json");
        /// <summary>
        /// Creates a ProblemContextManipulator for the application/problem+xml media type.
        /// </summary>
        /// <param name="typeRepresentations">The type representations that may transform an object into a Problem.</param>
        /// <returns>A ProblemContextManipulator instance for the application/problem+xml media type.</returns>
        public static ProblemContextManipulator Xml(ITypeRepresentations typeRepresentations) => new ProblemContextManipulator(typeRepresentations, "application/xml", "application/problem+xml");

        /// <summary>
        /// Sets the correct media type, if a Problem object is encountered.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <param name="response">The RestResponse.</param>
        public void ManipulateContext(HttpContext context, RestResponse response)
        {
            var typedHeaders = context.Response.GetTypedHeaders();
            if (typedHeaders.ContentType.MediaType == MediaType
                && response.UntypedResult is IHasRestValue rv
                && IsProblemType(rv.RestValue.Value?.GetType() ?? rv.RestValue.ValueType))
                typedHeaders.ContentType = new MediaTypeHeaderValue(ProblemMediaType);
        }

        private bool IsProblemType(Type valueType)
            => typeof(Problem).IsAssignableFrom(typeRepresentations.GetRepresentationType(valueType));


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeRepresentations">The type representations that may transform an object into a Problem.</param>
        /// <param name="mediaType">The incoming media type.</param>
        /// <param name="problemMediaType">The problem media type.</param>
        protected ProblemContextManipulator(ITypeRepresentations typeRepresentations, string mediaType, string problemMediaType)
        {
            this.typeRepresentations = typeRepresentations.AsTypeRepresentation();
            MediaType = mediaType;
            ProblemMediaType = problemMediaType;
        }

        private readonly ITypeRepresentation typeRepresentations;

        /// <summary>
        /// The incoming media type.
        /// </summary>
        public string MediaType { get; }
        /// <summary>
        /// The problem media type.
        /// </summary>
        public string ProblemMediaType { get; }

    }
}
