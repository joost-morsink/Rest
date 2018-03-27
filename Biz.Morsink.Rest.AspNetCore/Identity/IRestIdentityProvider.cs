using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore
{
    public interface IRestIdentityProvider : IIdentityProvider
    {
        IIdentity Parse(string path, bool nullOnFailure = false, RestPrefixContainer prefixes = null);
        IIdentity<object> ToGeneralIdentity(IIdentity id);
        IReadOnlyList<RestPath> GetRestPaths(Type forType);
        RestPrefixContainer Prefixes { get; }
    }
}