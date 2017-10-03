using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// This metadata describes the set of unique capability methods for some response to an OPTIONS request.
    /// </summary>
    public class Capabilities
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="methods">The set of methods allowed for the resource.</param>
        public Capabilities(IEnumerable<string> methods)
        {
            Methods = methods.ToArray();
        }
        /// <summary>
        /// Gets a collection of methods allowed for the resource.
        /// </summary>
        public IReadOnlyCollection<string> Methods { get; }
    }
}
