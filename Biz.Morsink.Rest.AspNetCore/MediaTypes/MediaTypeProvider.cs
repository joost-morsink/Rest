using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// Default IMediaTypeProvider implementation.
    /// </summary>
    internal class MediaTypeProvider : IMediaTypeProvider
    {
        private readonly IEnumerable<IMediaTypeMapping> mappings;
        private readonly Dictionary<Type, MediaType> mediaTypes;
        private readonly Dictionary<MediaType, Type> types;
        public MediaTypeProvider(IEnumerable<IMediaTypeMapping> mappings)
        {
            this.mappings = mappings;
            mediaTypes = mappings.SelectMany(x => x).ToDictionary(m => m.Type, m => m.MediaType);
            types = mappings.SelectMany(x => x).ToDictionary(m => m.MediaType, m => m.Type);
        }

        public MediaType? GetMediaType(Type original, Type representation)
            => GetMediaType(original) ?? GetMediaType(representation);

        public Type GetTypeForMediaType(MediaType mediaType)
            => types.TryGetValue(mediaType, out var type) ? type : default;

        private MediaType? GetMediaType(Type type)
            => mediaTypes.TryGetValue(type, out var mediaType) ? mediaType : default(MediaType?);
        
    }
}
