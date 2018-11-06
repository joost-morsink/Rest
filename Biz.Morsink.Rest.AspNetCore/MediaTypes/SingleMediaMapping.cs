using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    internal class SingleMediaMapping : IMediaTypeMapping
    {
        private readonly MediaTypeMapping mapping;

        public SingleMediaMapping(MediaType mediaType, Type type)
        {
            mapping = new MediaTypeMapping(mediaType, type);
        }
        public IEnumerator<MediaTypeMapping> GetEnumerator()
        {
            yield return mapping;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
