using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    /// <summary>
    /// This class serves as a bridge between the User as defined in ASP.Net core and the IUser interface that is needed for Rest request processing.
    /// </summary>
    public class AspNetCoreUser : IUser
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContextAccessor">An IHttpContextAccessor implementation, used to retrieve a Claims</param>
        public AspNetCoreUser(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the current user's ClaimsPrincipal.
        /// </summary>
        public ClaimsPrincipal Principal => httpContextAccessor.HttpContext.User;
    }
}
