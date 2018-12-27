using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// If failures cannot be returned from methods in the regular way, this exception can be used.
    /// </summary>
    public class RestFailureException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="failure">The Rest failure value.</param>
        public RestFailureException(IRestFailure failure) : base()
        {
            Failure = failure;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="failure">The Rest failure value.</param>
        /// <param name="message">A message for the exception.</param>
        public RestFailureException(IRestFailure failure, string message) : base(message)
        {
            Failure = failure;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="failure">The Rest failure value.</param>
        /// <param name="message">A message for the exception.</param>
        /// <param name="innerException">An inner exception.</param>
        public RestFailureException(IRestFailure failure, string message, Exception innerException) : base(message, innerException)
        {
            Failure = failure;
        }
        /// <summary>
        /// The Rest failure value.
        /// </summary>
        public IRestFailure Failure { get; }
    }
}
