using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest.AspNetCore
{
    public interface IRestIdentityProvider : IIdentityProvider
    {
        IIdentity Parse(string path, bool nullOnFailure = false);
        IIdentity<object> ToGeneralIdentity(IIdentity id);
    }
}