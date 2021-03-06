﻿using System;
using System.Runtime.Serialization;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// A generic exception for RestSerialization.
    /// </summary>
    [Serializable]
    public class RestSerializationException : Exception
    {
        public RestSerializationException()
        {
        }

        public RestSerializationException(string message) : base(message)
        {
        }

        public RestSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RestSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}