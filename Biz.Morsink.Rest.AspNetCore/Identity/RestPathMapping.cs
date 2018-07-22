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
        private static readonly Version VERSION_ONE = new Version(1, 0);
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="restPath">The Rest path.</param>
        /// <param name="componentTypes">The component types of the identity value.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query srtring.</param>
        /// <param name="version">A version for the Rest path.</param>
        public RestPathMapping(Type resourceType, string restPath, Type[] componentTypes = null, Type[] wildcardTypes = null, Version version = null)
        {
            ResourceType = resourceType;
            RestPath = restPath;
            ComponentTypes = componentTypes ?? new Type[] { resourceType };
            WildcardTypes = wildcardTypes;
            Version = version ?? VERSION_ONE;
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
        /// Gets a set of optional WildcardTypes for the query string.
        /// </summary>
        public Type[] WildcardTypes { get; }
        /// <summary>
        /// Gets the version for the Rest path mapping.
        /// </summary>
        public Version Version { get; }
    }
}
