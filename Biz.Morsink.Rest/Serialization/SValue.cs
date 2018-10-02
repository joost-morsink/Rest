using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    public class SValue : SItem
    {
        public static SValue Null { get; } = new SValue(null);
        public SValue(object value)
        {
            Value = value;
        }
        public SValue(object value, SFormat format)
        {
            Value = value;
            Format = format;
        }
        public object Value { get; }
        public SFormat Format { get; }

        public override int GetHashCode()
            => Value == null ? 0 : Value.GetHashCode();
        public override bool Equals(SItem other)
            => other is SValue val && Equals(val);
        public bool Equals(SValue other)
            => object.Equals(Value, other.Value);
    }
}
