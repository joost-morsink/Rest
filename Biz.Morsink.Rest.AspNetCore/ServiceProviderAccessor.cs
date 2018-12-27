using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// An accessor for IServiceProvider through the current HttpContext.
    /// </summary>
    public class ServiceProviderAccessor : IServiceProviderAccessor
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContextAccessor">An HTTP context accessor.</param>
        public ServiceProviderAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Gets the RequestServices as IServiceProvider from the HttpContext.
        /// </summary>
        public IServiceProvider ServiceProvider => httpContextAccessor.HttpContext.RequestServices;
    }
}
