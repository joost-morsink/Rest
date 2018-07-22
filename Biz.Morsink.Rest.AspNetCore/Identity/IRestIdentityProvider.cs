using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore
{
    public interface IRestIdentityProvider : IIdentityProvider
    {
        IEnumerable<(Version, Type)> GetSupportedVersions(Type type);
        IEnumerable<RestIdentityMatch> Match(string path, RestPrefixContainer prefixes = null);
        IIdentity Parse(string path, bool nullOnFailure = false, RestPrefixContainer prefixes = null, VersionMatcher versionMatcher = default);
        IIdentity<object> ToGeneralIdentity(IIdentity id);
        IReadOnlyList<RestPath> GetRestPaths(Type forType);
        RestPrefixContainer Prefixes { get; }
    }
}