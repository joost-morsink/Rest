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
        private readonly ConcurrentDictionary<Type, string> mediaTypes;
        public MediaTypeProvider(IEnumerable<IMediaTypeMapping> mappings)
        {
            this.mappings = mappings;
            mediaTypes = new ConcurrentDictionary<Type, string>();
        }

        public string GetMediaType(Type original, Type representation)
            => GetMediaType(original) ?? GetMediaType(representation);

        private string GetMediaType(Type type)
        {
            return mediaTypes.GetOrAdd(type, ty => mappings.Where(m => m.Applies(ty)).Select(m => m.GetMediaType(ty)).FirstOrDefault());
        }
    }
}
