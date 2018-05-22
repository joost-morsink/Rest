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
        private readonly IHttpContextAccessor contextAccessor;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contextAccessor">The HttpContextAccessor.</param>
        public CurrentHttpRestConverterAccessor(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }
        /// <summary>
        /// Gets the actual currently used IHttpRestConverter instance.
        /// </summary>
        public IHttpRestConverter CurrentHttpRestConverter 
            => contextAccessor.HttpContext.TryGetContextItem<IHttpRestConverter>(out var res) ? res : null;
    }
}
