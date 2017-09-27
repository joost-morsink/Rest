using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface for Rest failure results.
    /// </summary>
    public interface IRestFailure : IRestResult
    {
        /// <summary>
        /// Gets the reason for failure of the Rest request.
        /// </summary>
        RestFailureReason Reason { get; }
    }

}
