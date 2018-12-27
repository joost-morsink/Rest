using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Enum for Rest failure reasons.
    /// </summary>
    public enum RestFailureReason
    {
        /// <summary>
        /// The request was malformed or could not be validated.
        /// </summary>
        BadRequest,
        /// <summary>
        /// Something could not be found.
        /// </summary>
        NotFound,
        /// <summary>
        /// An error occurred. 
        /// Most likely this is due to an unexpected exception.
        /// </summary>
        Error,
        /// <summary>
        /// The action was not executed.
        /// Most likely this is due to the condition of a conditional request not being met.
        /// </summary>
        NotExecuted
    }
}
