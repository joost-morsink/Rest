using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IRestCollection : IHasIdentity
    {
        /// <summary>
        /// Contains the items in the current slice.
        /// </summary>
        IEnumerable<object> Items { get; }
        /// <summary>
        /// Gets the total number of items in the collection.
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Gets the maximum number of items in the slice.
        /// </summary>
        int? Limit { get; }
        /// <summary>
        /// Gets the number of entities that precede the entries in the slice.
        /// </summary>
        int Skip { get; }
    }
}
