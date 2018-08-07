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

        public object Value { get; }
    }
}
