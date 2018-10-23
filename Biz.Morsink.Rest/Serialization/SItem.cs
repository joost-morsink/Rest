using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// Abstract base class for items in intermediate serialization format.
    /// </summary>
    public abstract class SItem : IEquatable<SItem>
    {
        /// <summary>
        /// Must override GetHashCode!
        /// </summary>
        public abstract override int GetHashCode();

        public override bool Equals(object obj)
            => obj is SItem item && Equals(item);
        public abstract bool Equals(SItem other);

        public static bool operator ==(SItem left, SItem right)
            => ReferenceEquals(left, right) || left != null && left.Equals(right);
        public static bool operator !=(SItem left, SItem right)
            => !ReferenceEquals(left, right) && (left == null || !left.Equals(right));
        internal static string NewLine(int indent)
            => Environment.NewLine + new string(' ', indent);
        public override string ToString()
            => ToString(0);
        protected internal abstract string ToString(int indent);
    }
}
