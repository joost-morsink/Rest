using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    public class ResponseCache
    {
        public bool StoreAllowed { get; set; }
        public bool CacheAllowed { get; set; }
        public bool CachePrivate { get; set; }
        public TimeSpan Validity { get; set; }
        public string Token { get; set; }
    }
}
