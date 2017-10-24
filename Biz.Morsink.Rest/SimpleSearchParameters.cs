using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class represents a simple search criterion based on a query string parameter named 'q'.
    /// </summary>
    public class SimpleSearchParameters
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="q">The string value to search for.</param>
        public SimpleSearchParameters(string q)
        {
            Q = q;
        }
        /// <summary>
        /// Gets the string value to search for.
        /// </summary>
        public string Q { get; }
    }
}
