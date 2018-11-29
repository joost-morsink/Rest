using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// Metadata class used for matching version tokens.
    /// </summary>
    public class TokenMatching
    {
        /// <summary>
        /// Contains a list of tokens used for matching.
        /// </summary>
        public List<VersionToken> Tokens { get; set; }
        /// <summary>
        /// Indicates whether the one token should match.
        /// If false, none should match.
        /// </summary>
        public bool ShouldMatch { get; set; }
    }
}
