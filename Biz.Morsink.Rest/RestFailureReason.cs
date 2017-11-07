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
        BadRequest,
        NotFound,
        Error,
        NotExecuted
    }
}
