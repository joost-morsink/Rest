using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// Formatting indicator for properties and string values.
    /// </summary>
    public enum SFormat
    {
        /// <summary>
        /// Use default formatting.
        /// </summary>
        Default,
        /// <summary>
        /// Use the casing provided in the options.
        /// </summary>
        Cased,
        /// <summary>
        /// Use the literal string.
        /// </summary>
        Literal
    }
}
