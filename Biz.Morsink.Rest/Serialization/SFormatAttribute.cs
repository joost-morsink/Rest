using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class SFormatAttribute : Attribute
    {
        public SFormatAttribute() { }
        public SFormat Property { get; set; }
        public SFormat Value { get; set; }
    }
}
