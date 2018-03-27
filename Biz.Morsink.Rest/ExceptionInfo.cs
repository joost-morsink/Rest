using System;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// DTO class for exceptions. 
    /// Hides sensitive data such as stacktraces.
    /// </summary>
    public class ExceptionInfo
    {
        /// <summary>
        /// Creates an ExceptionInfo object based on an actual Exception.
        /// </summary>
        /// <param name="exception">The Exception.</param>
        /// <returns>An ExceptionInfo object that represents the Exception.</returns>
        public static ExceptionInfo Create(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            return new ExceptionInfo(exception.GetType().Name, exception.Message, exception.InnerException == null ? null : Create(exception.InnerException));
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The type of exception.</param>
        /// <param name="message">The exception's message.</param>
        /// <param name="inner">An inner ExceptionInfo object matching an InnerException.</param>
        public ExceptionInfo(string type, string message, ExceptionInfo inner = null)
        {
            Type = type;
            Message = message;
            Inner = inner;
        }
        /// <summary>
        /// Gets the type of the exception.
        /// </summary>
        public string Type { get; }
        /// <summary>
        /// Gets the message of the exception.
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// Gets the inner ExceptionInfo.
        /// This property can be null.
        /// </summary>
        public ExceptionInfo Inner { get; }
    }
}