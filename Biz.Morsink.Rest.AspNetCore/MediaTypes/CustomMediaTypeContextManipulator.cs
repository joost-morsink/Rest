using System;
using System.Collections.Generic;
using System.Text;
using Biz.Morsink.Rest.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// An Http context manipulator to set specific media types.
    /// </summary>
    public class CustomMediaTypeContextManipulator : IHttpContextManipulator
    {
        /// <summary>
        /// Creates a CustomMediaTypeContextManipulator for the application/json media type.
        /// </summary>
        /// <param name="mediaTypeProvider">A provider for media types.</param>
        /// <param name="typeRepresentations">A provider for type representations.</param>
        /// <returns>A CustomMediaTypeContextManipulator for the application/json media type.</returns>
        public static CustomMediaTypeContextManipulator Json(IMediaTypeProvider mediaTypeProvider, ITypeRepresentations typeRepresentations) => new CustomMediaTypeContextManipulator(mediaTypeProvider, typeRepresentations, "application/json", "+json");
        /// <summary>
        /// Creates a CustomMediaTypeContextManipulator for the application/xml media type.
        /// </summary>
        /// <param name="mediaTypeProvider">A provider for media types.</param>
        /// <param name="typeRepresentations">A provider for type representations.</param>
        /// <returns>A CustomMediaTypeContextManipulator for the application/xml media type.</returns>
        public static CustomMediaTypeContextManipulator Xml(IMediaTypeProvider mediaTypeProvider, ITypeRepresentations typeRepresentations) => new CustomMediaTypeContextManipulator(mediaTypeProvider, typeRepresentations, "application/xml", "+xml");

        private readonly IMediaTypeProvider mediaTypeProvider;
        private readonly ITypeRepresentation typeRepresentations;
        private readonly string suffix;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mediaTypeProvider">A provider for media types.</param>
        /// <param name="typeRepresentations">A provider for type representations.</param>
        /// <param name="mediaType">The media type this manipulator applies to.</param>
        /// <param name="suffix">The suffix used for this manipulator.</param>
        protected CustomMediaTypeContextManipulator(IMediaTypeProvider mediaTypeProvider, ITypeRepresentations typeRepresentations, string mediaType, string suffix)
        {
            this.mediaTypeProvider = mediaTypeProvider;
            this.typeRepresentations = typeRepresentations.AsTypeRepresentation();
            this.MediaType = mediaType;
            this.suffix = suffix;
        }
        /// <summary>
        /// The media type this manipulator applies to.
        /// </summary>
        public string MediaType { get; }

        public void ManipulateContext(HttpContext context, RestResponse response)
        {
            var typedHeaders = context.Response.GetTypedHeaders();

            if (typedHeaders.ContentType.MediaType == MediaType
                && response.UntypedResult is IHasRestValue rv)
            {
                var type = rv.RestValue.Value?.GetType() ?? rv.RestValue.ValueType;
                var mediaType = mediaTypeProvider.GetMediaType(type, typeRepresentations.GetRepresentationType(type));
                if (mediaType != null)
                    typedHeaders.ContentType = new MediaTypeHeaderValue(mediaType + suffix);
            }
        }
    }
}
