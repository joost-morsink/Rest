using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Accessor pattern interface for IServiceProvider.
    /// </summary>
    public interface IServiceProviderAccessor
    {
        /// <summary>
        /// Should return the currently applicable IServiceProvider instance.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}
