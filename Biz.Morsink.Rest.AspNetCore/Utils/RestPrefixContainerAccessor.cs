using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    /// <summary>
    /// This class implements the service for getting a scoped RestPrefixContainer by using the HttpContext's RequestServices.
    /// </summary>
    public class RestPrefixContainerAccessor : IRestPrefixContainerAccessor
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContextAccessor">An HttpContext accessor.</param>
        public RestPrefixContainerAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Gets the currently in scope RestPrefixContainer.
        /// </summary>
        public RestPrefixContainer RestPrefixContainer => httpContextAccessor.HttpContext.RequestServices.GetRequiredService<RestPrefixContainer>();
    }
}
