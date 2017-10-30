using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Authorization provider that allows everything.
    /// Warning: this results in an unsecured service.
    /// </summary>
    public class AlwaysAllowAuthorizationProvider : IAuthorizationProvider
    {
        /// <summary>
        /// Always returns true.
        /// </summary>
        public bool IsAllowed(ClaimsPrincipal principal, IIdentity id, string capability)
            => true;
    }
}
