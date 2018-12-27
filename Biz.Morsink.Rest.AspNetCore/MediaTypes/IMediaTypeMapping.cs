using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// Interface for a media type mapping.
    /// </summary>
    public interface IMediaTypeMapping : IEnumerable<MediaTypeMapping>
    {
    }
    /// <summary>
    /// A mapping of a media-type to a .Net type.
    /// </summary>
    public struct MediaTypeMapping
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mediaType">A media type.</param>
        /// <param name="type">A .Net type the media type maps to.</param>
        public MediaTypeMapping(MediaType mediaType, Type type)
        {
            MediaType = mediaType;
            Type = type;
        }
        /// <summary>
        /// A media type.
        /// </summary>
        public MediaType MediaType { get; }
        /// <summary>
        /// Contains the .Net the media type maps to.
        /// </summary>
        public Type Type { get; }
    }
}
