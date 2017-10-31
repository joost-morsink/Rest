using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A cache result helper type.
    /// </summary>
    public struct CacheResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response">The underlying RestResponse.</param>
        public CacheResult(RestResponse response)
        {
            Response = response;
        }
        /// <summary>
        /// True if this CacheResult contains a Response.
        /// </summary>
        public bool IsSuccesful => Response != null;
        /// <summary>
        /// Gets the response contained in this CacheResult.
        /// </summary>
        public RestResponse Response { get; }
    }
}
