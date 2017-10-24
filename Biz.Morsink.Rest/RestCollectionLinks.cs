using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A dynamic link provider for collections.
    /// Takes care of first, last, prev and next links for collection slices.
    /// </summary>
    /// <typeparam name="T">The collection type.</typeparam>
    /// <typeparam name="E">The entity type contained in the collection.</typeparam>
    public class RestCollectionLinks<T,E> : IDynamicLinkProvider<T>
        where T: RestCollection<E>
    {
        /// <summary>
        /// The reltype 'first'.
        /// </summary>
        public const string first = nameof(first);
        /// <summary>
        /// The reltype 'last'.
        /// </summary>
        public const string last = nameof(last);
        /// <summary>
        /// The reltype 'prev'.
        /// </summary>
        public const string prev = nameof(prev);
        /// <summary>
        /// The reltype 'next'.
        /// </summary>
        public const string next = nameof(next);
        /// <summary>
        /// The collection parameter name 'skip'.
        /// </summary>
        public const string skip = nameof(skip);
        /// <summary>
        /// The collection parameter name 'limit'.
        /// </summary>
        public const string limit = nameof(limit);

        /// <summary>
        /// Gets the links for the collection slice.
        /// </summary>
        /// <param name="resource">The collection slice.</param>
        /// <returns>A list of links that apply to the collection slice.</returns>
        public IReadOnlyList<Link> GetLinks(T resource)
        {
            var res = new List<Link>();
            var conv = resource.Id.Provider.GetConverter(typeof(T), false).Convert(resource.Id.Value);
            var dict = conv.To<Dictionary<string, string>>().ToImmutableDictionary();
            var cp = conv.To<CollectionParameters>();
            if (cp.Limit.HasValue)
            {
                res.Add(Link.Create(first, FreeIdentity<T>.Create(
                    dict.SetItem(limit, cp.Limit.Value.ToString())
                    .SetItem(skip, "0")
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
                res.Add(Link.Create(last, FreeIdentity<T>.Create(
                    dict.SetItem(limit, cp.Limit.Value.ToString())
                    .SetItem(skip, ((resource.Count - 1) / cp.Limit.Value * cp.Limit.Value).ToString())
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
                if (cp.Skip > 0)
                    res.Add(Link.Create(prev, FreeIdentity<T>.Create(
                        dict.SetItem(limit, cp.Limit.Value.ToString())
                        .SetItem(skip, Math.Max(0, cp.Skip - cp.Limit.Value).ToString())
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
                if (cp.Skip + cp.Limit.Value < resource.Count)
                    res.Add(Link.Create(next, FreeIdentity<T>.Create(
                        dict.SetItem(limit, cp.Limit.Value.ToString())
                        .SetItem(skip, (cp.Skip + cp.Limit.Value).ToString())
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
            }
            return res;
        }
    }
}
