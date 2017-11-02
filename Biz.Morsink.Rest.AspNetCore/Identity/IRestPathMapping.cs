using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    /// <summary>
    /// Represents mapping of a resource type to a Rest path.
    /// </summary>
    public interface IRestPathMapping
    {
        /// <summary>
        /// Gets the resource type.
        /// </summary>
        Type ResourceType { get; }
        /// <summary>
        /// Gets the Rest path.
        /// </summary>
        string RestPath { get; }
        /// <summary>
        /// Gets the component types of the identity value.
        /// </summary>
        Type[] ComponentTypes { get; }
        /// <summary>
        /// Gets an optional WildcardType for the query string.
        /// </summary>
        Type WildcardType { get; }
    }
}
