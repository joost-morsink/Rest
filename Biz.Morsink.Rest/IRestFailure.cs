﻿using System;
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
        /// <summary>
        /// Gets the kind of Rest entity the failure occurred on.
        /// </summary>
        RestEntityKind FailureOn { get; }
        /// <summary>
        /// Converts the failure value to a typed result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        RestResult<T> Select<T>();
    }

}
