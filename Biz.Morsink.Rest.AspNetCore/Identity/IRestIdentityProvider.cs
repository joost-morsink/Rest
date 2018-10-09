using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// The interface for Rest identity providers.
    /// </summary>
    public interface IRestIdentityProvider : IIdentityProvider
    {
        /// <summary>
        /// Gets all related types and version for which these types implement the 'same' repository.
        /// </summary>
        /// <param name="type">The type to check for related versions.</param>
        /// <returns>A list of version type pairs.</returns>
        IEnumerable<(Version, Type)> GetSupportedVersions(Type type);
        /// <summary>
        /// Parses a path and matches versions of rest repositories to the parsed path.
        /// </summary>
        /// <param name="path">The path to parse.</param>
        /// <param name="prefixes">A container of curie prefixes.</param>
        /// <returns>A list of identity matches.</returns>
        IEnumerable<RestIdentityMatch> Match(string path, RestPrefixContainer prefixes = null);
        /// <summary>
        /// Parses a rest path and matches up the right version.
        /// </summary>
        /// <param name="path">The path to parse.</param>
        /// <param name="nullOnFailure">If true, returns a null on failure, otherwise it will return an IIdentity&lt;object&gt;</param>
        /// <param name="prefixes">A container of curie prefixes.</param>
        /// <param name="versionMatcher">A version matcher to resolve ambiguous rest paths.</param>
        /// <returns>An identity value for the specified path.</returns>
        IIdentity Parse(string path, bool nullOnFailure = false, RestPrefixContainer prefixes = null, VersionMatcher versionMatcher = default);
        /// <summary>
        /// Parses a rest path when type information is already known.
        /// </summary>
        /// <param name="path">The path to parse.</param>
        /// <param name="specific">The entity type.</param>
        /// <param name="prefixes">A container of curie prefixes.</param>
        /// <returns>An identity value for the specified path.</returns>
        IIdentity Parse(string path, Type specific, RestPrefixContainer prefixes = null);
        /// <summary>
        /// Parses a rest path when type information is already known.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="path">The path to parse.</param>
        /// <param name="prefixes">A container of curie prefixes.</param>
        /// <returns>An identity value for the specified path.</returns>
        IIdentity<T> Parse<T>(string path,  RestPrefixContainer prefixes = null);
        /// <summary>
        /// Converts an identity value to a general IIdentity&lt;object&gt; value
        /// </summary>
        /// <param name="id">The identity value to convert.</param>
        /// <returns>An untyped identity value.</returns>
        IIdentity<object> ToGeneralIdentity(IIdentity id);
        /// <summary>
        /// Gets a collection of paths for some resource type.
        /// </summary>
        /// <param name="forType">The resource type to get the paths for.</param>
        /// <returns>A list of Rest paths.</returns>
        IReadOnlyList<RestPath> GetRestPaths(Type forType);
        /// <summary>
        /// Contains a default collection of curie prefixes for the identity provider.
        /// </summary>
        RestPrefixContainer Prefixes { get; }
    }
}