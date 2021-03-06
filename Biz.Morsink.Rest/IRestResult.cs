﻿using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface for Rest results.
    /// </summary>
    public interface IRestResult
    {
        /// <summary>
        /// Wraps the result into a response.
        /// </summary>
        /// <param name="metadata">Metadata for the response.</param>
        /// <returns>A response object containing the result and the metadata.</returns>
        RestResponse ToResponse(TypeKeyedDictionary metadata = null);
        /// <summary>
        /// The Linq Select method for Rest results.
        /// As a Rest result is a container for a Rest value, this function can manipulate the inner value.
        /// </summary>
        /// <param name="f">The function for Rest value manipulation.</param>
        /// <returns></returns>
        IRestResult Select(Func<IRestValue, IRestValue> f);
        /// <summary>
        /// True if the Rest result is a success value.
        /// </summary>
        bool IsSuccess { get; }
        /// <summary>
        /// True if the Rest result is a failure.
        /// </summary>
        bool IsFailure { get; }
        /// <summary>
        /// True if the Rest result is a redirect.
        /// </summary>
        bool IsRedirect { get; }
        /// <summary>
        /// True if the Rest result is still pending.
        /// </summary>
        bool IsPending { get; }
        /// <summary>
        /// Tries to cast this Rest result into a Rest success.
        /// </summary>
        /// <returns>An IRestSuccess instance if the Rest result is successful, null otherwise.</returns>
        IRestSuccess AsSuccess();
        /// <summary>
        /// Tries to cast this Rest result into a Rest failure.
        /// </summary>
        /// <returns>An IRestFailure instance if the Rest result is a failure, null otherwise.</returns>
        IRestFailure AsFailure();
        /// <summary>
        /// Tries to cast this Rest result into a Rest redirect.
        /// </summary>
        /// <returns>An IRestRedirect instance if the Rest result is a redirect, null otherwise.</returns>
        IRestRedirect AsRedirect();
        /// <summary>
        /// Tries to cast this Rest reslut into a Rest pending.
        /// </summary>
        IRestPending AsPending();
        /// <summary>
        /// Makes this result into a redirect result of type NotNecessary.
        /// Used if Version tokens match.
        /// </summary>
        /// <returns>An IRestRedirect instance of type NotNecessary.</returns>
        IRestRedirect MakeNotNecessary();
    }
}
