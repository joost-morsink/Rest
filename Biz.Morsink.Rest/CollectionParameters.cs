using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class represents parameters of a Rest collection. 
    /// These properties should be contained within the collection's identity value.
    /// </summary>
    public class CollectionParameters
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="limit">The maximum number of returned results in a collection instance.</param>
        /// <param name="skip">The amount of objects to skip.</param>
        public CollectionParameters(int? limit = null, int skip = 0)
        {
            Limit = limit;
            Skip = skip;
        }
        /// <summary>
        /// Gets the maximum number of returned results in a collection instance.
        /// </summary>
        public int? Limit { get; }
        /// <summary>
        /// Gets the amount of objects to skip.
        /// </summary>
        public int Skip { get; }
    }
 
}
