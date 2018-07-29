using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Exception for the Http 415 status.
    /// </summary>
    public class UnsupportedMediaTypeException : HttpException
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
        /// <summary>
        /// The status code for this exception is 415.
        /// </summary>
        public override int StatusCode => (int)HttpStatusCode.UnsupportedMediaType;
    }
}
