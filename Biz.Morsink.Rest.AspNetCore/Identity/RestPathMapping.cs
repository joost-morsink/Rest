using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    /// <summary>
    /// Implementation of the IRestPathMapping interface.
    /// Represents mapping of a resource type to a Rest path.
    /// </summary>
    public class RestPathMapping : IRestPathMapping
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="restPath">The Rest path.</param>
        /// <param name="componentTypes">The component types of the identity value.</param>
        /// <param name="wildcardType">An optional wildcard type for the query srtring.</param>
        public RestPathMapping(Type resourceType, string restPath, Type[] componentTypes = null, Type wildcardType = null)
        {
            ResourceType = resourceType;
            RestPath = restPath;
            ComponentTypes = componentTypes ?? new Type[] { resourceType };
            WildcardType = wildcardType;
        }
        /// <summary>
        /// Gets the resource type.
        /// </summary>
        public Type ResourceType { get; }
        /// <summary>
        /// Gets the Rest path.
        /// </summary>
        public string RestPath { get; }
        /// <summary>
        /// Gets the component types of the identity value.
        /// </summary>
        public Type[] ComponentTypes { get; }
        /// <summary>
        /// Gets an optional WildcardType for the query string.
        /// </summary>
        public Type WildcardType { get; }
    }
}
