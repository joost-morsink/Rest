using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This interface specifies an accessor pattern for the IRestRequestScope interface.
    /// </summary>
    public interface IRestRequestScopeAccessor
    {
        /// <summary>
        /// Gets the current Rest request scope.
        /// </summary>
        IRestRequestScope Scope { get; }
    }
}
