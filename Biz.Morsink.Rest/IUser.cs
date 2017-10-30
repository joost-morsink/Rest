using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface that gives access to the User in the context of the application.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Gets the ClaimsPrincipal for the current User.
        /// </summary>
        ClaimsPrincipal Principal { get; }
    }
}
