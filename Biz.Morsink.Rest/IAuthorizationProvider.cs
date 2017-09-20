using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface to determine if authorization allows a principal some capability on a certain resource identity.
    /// </summary>
    public interface IAuthorizationProvider
    {
        /// <summary>
        /// Determines if authorization allows a principal some capability on a certain resource identity.
        /// </summary>
        /// <param name="principal">An authenticated principal.</param>
        /// <param name="id">A resource identity value.</param>
        /// <param name="capability">The capability to be performed on the resource.</param>
        /// <returns>True if the principal is allowed to perform the capability on the resource.</returns>
        bool IsAllowed(ClaimsPrincipal principal, IIdentity id, Type capability);
    }
}
