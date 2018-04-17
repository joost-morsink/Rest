using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Exception for the Http 415 status.
    /// </summary>
    public class UnsupportedMediaTypeException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public UnsupportedMediaTypeException() : base() { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">A message for the exception.</param>
        public UnsupportedMediaTypeException(string message) : base(message) { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">A message for the exception.</param>
        /// <param name="innerException">An inner exception.</param>
        public UnsupportedMediaTypeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
