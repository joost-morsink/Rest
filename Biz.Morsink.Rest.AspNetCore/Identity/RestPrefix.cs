using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    /// <summary>
    /// A Rest URI prefix.
    /// </summary>
    public class RestPrefix
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prefix">The URI prefix.</param>
        /// <param name="abbreviation">An abbreviation for the URI prefix.</param>
        public RestPrefix(string prefix, string abbreviation)
        {
            Prefix = prefix;
            Abbreviation = abbreviation;
        }
        /// <summary>
        /// Contains the URI prefix.
        /// </summary>
        public string Prefix { get; }
        /// <summary>
        /// Contains an abbreviation for the prefix.
        /// </summary>
        public string Abbreviation { get; }
    }

}
