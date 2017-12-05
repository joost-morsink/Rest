using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// A class containing a version token.
    /// This token may be used to revalidate a resource server-side or to conditionally execute requests.
    /// </summary>
    public class VersionToken : IEquatable<VersionToken>
    {
        /// <summary>
        /// A token identifying the version of the resource. 
        /// This token may be used to revalidate a resource server-side or to conditionally execute requests.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Indicates whether the token may be considered 'strong' or 'weak'.
        /// Strong tokens guarantee binary equality of representations, while weak ones only guarantee equivalence of representations.
        /// </summary>
        public bool IsStrong { get; set; }

        public override int GetHashCode()
            => (int)(IsStrong ? 0xaaaaaaaa : 0x55555555) ^ Token.GetHashCode();
        public override bool Equals(object obj)
            => Equals((VersionToken)obj);
        public bool Equals(VersionToken other)
            => !ReferenceEquals(other,null) && IsStrong == other.IsStrong && Token == other.Token;
        public static bool operator ==(VersionToken x, VersionToken y)
            => x.Equals(y);
        public static bool operator !=(VersionToken x, VersionToken y)
            => !x.Equals(y);
    }
}
