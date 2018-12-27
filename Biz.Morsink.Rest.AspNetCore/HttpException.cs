using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// An abstract class for exceptions that carry an HTTP status code.
    /// </summary>
    public abstract class HttpException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpException() : base() { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">A message.</param>
        public HttpException(string message) : base(message) { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">A message.</param>
        /// <param name="inner">An inner exception.</param>
        public HttpException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Constructor for serialization purposes.
        /// </summary>
        /// <param name="info">A SerializationInfo instance.</param>
        /// <param name="context">The StreamingContext.</param>
        public HttpException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        /// <summary>
        /// The HTTP status code that should be sent on occurrence of this exception.
        /// </summary>
        public abstract int StatusCode { get; }
    }
}
