using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Service interface to get the currently used IHttpRestConverter instance.
    /// </summary>
    public interface ICurrentHttpRestConverterAccessor
    {
        /// <summary>
        /// Gets the actual currently used IHttpRestConverter instance.
        /// </summary>
        IHttpRestConverter CurrentHttpRestConverter { get; }
    }
}
