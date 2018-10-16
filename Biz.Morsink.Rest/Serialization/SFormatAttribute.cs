using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// Attribute to indicate formatting on a property or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class SFormatAttribute : Attribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SFormatAttribute() { }
        /// <summary>
        /// Formatting to apply to the property name.
        /// </summary>
        public SFormat Property { get; set; }
        /// <summary>
        /// Formatting to apply to the value.
        /// </summary>
        public SFormat Value { get; set; }
    }
}
