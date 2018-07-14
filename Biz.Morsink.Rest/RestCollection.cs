using Biz.Morsink.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Base class for collection (slice) types in Restful apis.
    /// A collection represents the entire collection of entities that exist at any given time.
    /// Using identity values criteria may be passed, effectively creating slices of the entire collection.
    /// </summary>
    /// <typeparam name="T">The entity type contained in the collection.</typeparam>
    /// <typeparam name="I">The type whose instances contain descriptive information about instances of the entity type.</typeparam>
    public class RestCollection<T, I> : IRestCollection
        where I : IHasIdentity<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The collection's identity value.</param>
        /// <param name="items">The items in the current slice.</param>
        /// <param name="count">The total number of items in the collection (slice).</param>
        /// <param name="limit">The maximum number of items in the slice. Default = null.</param>
        /// <param name="skip">The number of entities to skip before containing entities in the slice. Default = 0.</param>
        public RestCollection(IIdentity id, IEnumerable<I> items, int count, int? limit = null, int skip = 0)
        {
            Id = id;
            Items = items;
            Count = count;
            Limit = limit;
            Skip = skip;
        }
        /// <summary>
        /// Gets the collection's identity value.
        /// </summary>
        public IIdentity Id { get; }
        /// <summary>
        /// Gets the items in the current slice.
        /// </summary>
        public IEnumerable<I> Items { get; }
        IEnumerable<object> IRestCollection.Items => Items.Cast<object>();
        /// <summary>
        /// Gets the total number of items in the collection (slice).
        /// </summary>
        public int Count { get; }
        /// <summary>
        /// Gets the maximum number of items in the slice.
        /// </summary>
        public int? Limit { get; }
        /// <summary>
        /// Gets the number of entities that precede the entries in the slice.
        /// </summary>
        public int Skip { get; }
    }
    /// <summary>
    /// Base class for collection (slice) types in Restful apis.
    /// A collection represents the entire collection of entities that exist at any given time.
    /// Using identity values criteria may be passed, effectively creating slices of the entire collection.
    /// The descriptive type is set to the entity type itself in this base class.
    /// </summary>
    /// <typeparam name="T">The entity type contained in the collection.</typeparam>
    public class RestCollection<T> : RestCollection<T, T>
        where T : IHasIdentity<T>
    {
        public RestCollection(IIdentity id, IEnumerable<T> items, int count, int? limit = null, int skip = 0)
            : base(id, items, count, limit, skip) { }
    }
}
