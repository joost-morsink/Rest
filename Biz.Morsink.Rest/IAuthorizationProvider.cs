using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IAuthorizationProvider
    {
        bool IsAllowed(ClaimsPrincipal principal, IIdentity id, Type capability);
    }
}
