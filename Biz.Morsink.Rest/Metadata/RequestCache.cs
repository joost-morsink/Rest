using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    public class RequestCache
    {
        public string Token { get; set; }
        public bool ShouldMatch { get; set; }
    }
}
