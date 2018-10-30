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
        public static ProblemContextManipulator Json(IEnumerable<ITypeRepresentation> typeRepresentations) => new ProblemContextManipulator(typeRepresentations, "application/json", "application/problem+json");
        /// <summary>
        /// Creates a ProblemContextManipulator for the application/problem+xml media type.
        /// </summary>
        /// <param name="typeRepresentations">The type representations that may transform an object into a Problem.</param>
        /// <returns>A ProblemContextManipulator instance for the application/problem+xml media type.</returns>
        public static ProblemContextManipulator Xml(IEnumerable<ITypeRepresentation> typeRepresentations) => new ProblemContextManipulator(typeRepresentations, "application/xml", "application/problem+xml");

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
            => typeof(Problem).IsAssignableFrom(valueType)
               || isProblem.GetOrAdd(valueType,
                ty => typeRepresentations
                    .Select(tr => tr.GetRepresentationType(ty))
                    .Where(rept => rept != null)
                    .Take(1)
                    .Any(rept => typeof(Problem).IsAssignableFrom(rept)));


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeRepresentations">The type representations that may transform an object into a Problem.</param>
        /// <param name="mediaType">The incoming media type.</param>
        /// <param name="problemMediaType">The problem media type.</param>
        protected ProblemContextManipulator(IEnumerable<ITypeRepresentation> typeRepresentations, string mediaType, string problemMediaType)
        {
            this.typeRepresentations = typeRepresentations;
            MediaType = mediaType;
            ProblemMediaType = problemMediaType;
            isProblem = new ConcurrentDictionary<Type, bool>();
        }

        private readonly IEnumerable<ITypeRepresentation> typeRepresentations;
        private readonly ConcurrentDictionary<Type, bool> isProblem;

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
