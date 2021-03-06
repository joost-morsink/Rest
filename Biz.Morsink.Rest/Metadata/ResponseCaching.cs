﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// Metadata class concerning caching information in a response.
    /// </summary>
    public class ResponseCaching
    {
        /// <summary>
        /// True if the client is allowed to store a cached result.
        /// </summary>
        public bool StoreAllowed { get; set; }
        /// <summary>
        /// True if the client is allowed to use a cached result. 
        /// If not, a cached version must be revalidated server-side.
        /// </summary>
        public bool CacheAllowed { get; set; }
        /// <summary>
        /// True if the response contains data that is specific for the client session.
        /// </summary>
        public bool CachePrivate { get; set; }
        /// <summary>
        /// Contains a TimeSpan of the validity of the cached result.
        /// </summary>
        public TimeSpan Validity { get; set; }
        /// <summary>
        /// Make a copy of the current object
        /// </summary>
        /// <returns></returns>
        public ResponseCaching Copy()
            => new ResponseCaching
            {
                CacheAllowed = CacheAllowed,
                CachePrivate = CachePrivate,
                StoreAllowed = StoreAllowed,
                Validity = Validity
            };
    }
}
