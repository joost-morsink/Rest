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
    public struct MediaTypeMapping
    {
        public MediaTypeMapping(MediaType mediaType, Type type)
        {
            MediaType = mediaType;
            Type = type;
        }
        public MediaType MediaType { get; }
        public Type Type { get; }
    }
}
