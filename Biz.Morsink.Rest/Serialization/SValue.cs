using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// A primitive value in intermediate serialization format.
    /// </summary>
    public class SValue : SItem
    {
        /// <summary>
        /// Singleton property for the null value.
        /// </summary>
        public static SValue Null { get; } = new SValue(null);
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">The primitive value.</param>
        public SValue(object value)
        {
            Value = value;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">The primitive value.</param>
        /// <param name="format">The formatting for the value.</param>
        public SValue(object value, SFormat format)
        {
            Value = value;
            Format = format;
        }
        /// <summary>
        /// The actual value.
        /// </summary>
        public object Value { get; }
        /// <summary>
        /// The formatting for the value.
        /// </summary>
        public SFormat Format { get; }

        public override int GetHashCode()
            => Value == null ? 0 : Value.GetHashCode();
        public override bool Equals(SItem other)
            => other is SValue val && Equals(val);
        public bool Equals(SValue other)
            => object.Equals(Value, other.Value);
        protected internal override string ToString(int indent)
            => $"{Value}";
    }
}
