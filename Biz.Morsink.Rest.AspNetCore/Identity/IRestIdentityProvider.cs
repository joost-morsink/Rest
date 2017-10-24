using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest.AspNetCore
{
    public interface IRestIdentityProvider : IIdentityProvider
    {
        IIdentity Parse(string path, bool nullOnFailure = false);
        IIdentity<object> ToGeneralIdentity(IIdentity id);
        IReadOnlyList<RestPath> GetRestPaths(Type forType);

    }
}