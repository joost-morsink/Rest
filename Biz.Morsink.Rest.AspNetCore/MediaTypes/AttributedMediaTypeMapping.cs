using Biz.Morsink.Rest.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// A media type mapping implementation for classes attributed with the MediaTypeAttribute.
    /// </summary>
    internal class AttributedMediaTypeMapping : IMediaTypeMapping
    {
        private readonly IEnumerable<MediaTypeMapping> allTypes;

        public AttributedMediaTypeMapping(IEnumerable<IRestRepository> repositories, ITypeDescriptorCreator typeDescriptorCreator)
        {
            var apiDescription = new RestApiDescription(repositories, typeDescriptorCreator);
            allTypes = apiDescription.EntityTypes.Select(grp => grp.Key)
                .Select(type => (type, type.GetTypeInfo().GetCustomAttribute<MediaTypeAttribute>()?.MediaType))
                .Where(t => t.MediaType != null)
                .Select(t => new MediaTypeMapping(t.MediaType, t.type))
                .ToArray();
        }

        public IEnumerator<MediaTypeMapping> GetEnumerator()
            => allTypes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
