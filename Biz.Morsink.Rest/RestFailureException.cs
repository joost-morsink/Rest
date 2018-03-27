using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class RestFailureException : Exception
    {
        public RestFailureException(IRestFailure failure) : base()
        {
            Failure = failure;
        }
        public RestFailureException(IRestFailure failure, string message) : base(message)
        {
            Failure = failure;
        }
        public RestFailureException(IRestFailure failure, string message, Exception innerException) : base(message, innerException)
        {
            Failure = failure;
        }
        public IRestFailure Failure { get; }
    }
}
