using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    /// <summary>
    /// This struct represents a match with an entry in the RestIdentityProvider.
    /// </summary>
    public struct RestIdentityMatch
    { 
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="match">The RestPath match.</param>
        /// <param name="forType">The resource type.</param>
        /// <param name="componentTypes">The component resource types.</param>
        /// <param name="wildcardTypes">Optional array of wildcard types (query string).</param>
        /// <param name="version">A version for the match.</param>
        public RestIdentityMatch(RestPath.Match match, Type forType, Type[] componentTypes, Type[] wildcardTypes, Version version)
        {
            Match = match;
            ForType = forType;
            ComponentTypes = componentTypes;
            WildcardTypes = wildcardTypes;
            Version = version;
        }
        /// <summary>
        /// True if the match was successful.
        /// </summary>
        public bool IsSuccessful => Match.IsSuccessful;
        /// <summary>
        /// The RestPath match.
        /// </summary>
        public RestPath.Match Match { get; }
        /// <summary>
        /// The RestPath that was matched.
        /// </summary>
        public RestPath Path => Match.Path;
        /// <summary>
        /// The resource type.
        /// </summary>
        public Type ForType { get; }
        /// <summary>
        /// The resource components type.
        /// </summary>
        public Type[] ComponentTypes { get; }
        /// <summary>
        /// An optional array of wildcard types (query string).
        /// </summary>
        public Type[] WildcardTypes { get; }
        /// <summary>
        /// A version for this match.
        /// </summary>
        public Version Version { get; }
    }

}
