using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// Metadata class concerning request caching.
    /// </summary>
    public class RequestCache
    {
        /// <summary>
        /// Contains a token that identifies the version of the cached resource at the client
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// True if the request should be fulfilled when the version is still actual, false if ot should be fulfilled if it is not actual.
        /// </summary>
        public bool ShouldMatch { get; set; }
    }
}
