using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    public abstract class SItem : IEquatable<SItem>
    {
        public abstract override int GetHashCode();

        public override bool Equals(object obj)
            => obj is SItem item && Equals(item);
        public abstract bool Equals(SItem other);

        public static bool operator ==(SItem left, SItem right)
            => ReferenceEquals(left, right) || left != null && left.Equals(right);
        public static bool operator !=(SItem left, SItem right)
            => !ReferenceEquals(left, right) && (left == null || !left.Equals(right));
    }
}
