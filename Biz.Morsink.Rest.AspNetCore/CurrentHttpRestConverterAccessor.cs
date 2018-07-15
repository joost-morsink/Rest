using Biz.Morsink.Rest.AspNetCore.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Service interface to get the currently used IHttpRestConverter instance.
    /// </summary>
    public class CurrentHttpRestConverterAccessor : ICurrentHttpRestConverterAccessor
    {
        private readonly IRestRequestScopeAccessor scopeAccessor;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contextAccessor">The HttpContextAccessor.</param>
        public CurrentHttpRestConverterAccessor(IRestRequestScopeAccessor scopeAccessor)
        {
            this.scopeAccessor = scopeAccessor;
        }
        /// <summary>
        /// Gets the actual currently used IHttpRestConverter instance.
        /// </summary>
        public IHttpRestConverter CurrentHttpRestConverter
            => scopeAccessor.Scope.TryGetScopeItem<IHttpRestConverter>(out var res) ? res : null;
    }
}
