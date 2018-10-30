using System;
using System.Reflection;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// A media type mapping implementation for classes attributed with the MediaTypeAttribute.
    /// </summary>
    internal class AttributedMediaTypeMapping : IMediaTypeMapping
    {
        public bool Applies(Type type)
            => type.GetTypeInfo().GetCustomAttribute<MediaTypeAttribute>() != null;

        public string GetMediaType(Type type)
            => type.GetTypeInfo().GetCustomAttribute<MediaTypeAttribute>()?.MediaType;
    }
}
