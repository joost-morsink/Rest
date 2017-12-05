using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    public class TokenMatching
    {
        public List<VersionToken> Tokens { get; set; }
        public bool Matches { get; set; }
    }
}
