using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    public abstract class HttpException : Exception
    {
        public HttpException() : base() { }
        public HttpException(string message) : base(message) { }
        public HttpException(string message, Exception inner) : base(message, inner) { }
        public HttpException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public abstract int StatusCode { get; }
    }
}
