using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    public class RestPathMapping : IRestPathMapping
    {
        public RestPathMapping(Type resourceType, string restPath, Type[] componentTypes = null, Type wildcardType = null)
        {
            ResourceType = resourceType;
            RestPath = restPath;
            ComponentTypes = componentTypes ?? new Type[] { resourceType };
            WildcardType = wildcardType;
        }
        public Type ResourceType { get; }

        public string RestPath { get; }

        public Type[] ComponentTypes { get; }

        public Type WildcardType { get; }
    }
}
