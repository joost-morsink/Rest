using Biz.Morsink.Rest.Utils;
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
        private readonly IRestRequestScopeAccessor scopeAccessor;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scopeAccessor">An HttpContext accessor.</param>
        public RestPrefixContainerAccessor(IRestRequestScopeAccessor scopeAccessor)
        {
            this.scopeAccessor = scopeAccessor;
        }
        /// <summary>
        /// Gets the currently in scope RestPrefixContainer.
        /// </summary>
        public RestPrefixContainer RestPrefixContainer => scopeAccessor.Scope.GetScopeItem<RestPrefixContainer>();
    }
}
