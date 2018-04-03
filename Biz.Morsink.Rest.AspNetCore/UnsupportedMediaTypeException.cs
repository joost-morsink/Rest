using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    public class UnsupportedMediaTypeException : Exception
    {
        public UnsupportedMediaTypeException() : base() { }
        public UnsupportedMediaTypeException(string message) : base(message) { }
        public UnsupportedMediaTypeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
