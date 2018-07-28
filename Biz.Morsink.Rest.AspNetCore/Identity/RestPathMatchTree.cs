using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Utility class to match RestPaths in a tree.
    /// </summary>
    public class RestPathMatchTree<T>
    {
        private class SegmentEqualityComparer : IEqualityComparer<RestPath.Segment>
        {
            private SegmentEqualityComparer() { }
            public static SegmentEqualityComparer Instance { get; } = new SegmentEqualityComparer();
            public bool Equals(RestPath.Segment x, RestPath.Segment y)
                => x.IsWildcard && y.IsWildcard
                || !x.IsWildcard && !y.IsWildcard && x.Content == y.Content;

            public int GetHashCode(RestPath.Segment obj)
                => obj.IsWildcard ? 0 : obj.Content.GetHashCode();
        }
        private readonly ILookup<RestPath.Segment, (RestPath, T)> lookup;
        private readonly ConcurrentDictionary<RestPath.Segment, RestPathMatchTree<T>> tree;
        private readonly string localPrefix;

        /// <summary>
        /// Constructs a matching tree based on a collection of paths
        /// </summary>
        public RestPathMatchTree(IEnumerable<(RestPath, T)> paths, string localPrefix)
        {
            lookup = paths.Where(p => p.Item1.Count > 0).ToLookup(p => p.Item1[0], SegmentEqualityComparer.Instance);
            tree = new ConcurrentDictionary<RestPath.Segment, RestPathMatchTree<T>>();
            this.localPrefix = localPrefix;
            Terminals = paths.Where(p => p.Item1.Count == 0).ToArray();
        }

        /// <summary>
        /// Gets all the terminal paths at the current location in the tree.
        /// </summary>
        public IReadOnlyList<(RestPath, T)> Terminals { get; }

        /// <summary>
        /// Tries to navigate the tree.
        /// </summary>
        public RestPathMatchTree<T> this[RestPath.Segment segment]
        {
            get
            {
                if (tree.TryGetValue(segment, out var val))
                    return val;
                else
                {
                    var paths = lookup[segment];
                    if (paths.Any())
                        return tree.GetOrAdd(segment, _ => new RestPathMatchTree<T>(paths.Select(p => (p.Item1.Skip(), p.Item2)), localPrefix));
                    else
                        return segment.IsWildcard ? null : this[RestPath.Segment.Wildcard];
                }
            }
        }
        /// <summary>
        /// Try to match a certain path to some path in the tree.
        /// </summary>
        public IEnumerable<(RestPath.Match, T)> Walk(RestPath path)
        {
            if (path.Count == 0)
                return Terminals.Select(p => (p.Item1.GetFullPath().MatchPath(path.GetFullPath(), localPrefix), p.Item2))
                    .Where(m => m.Item1.IsSuccessful);
            else
                return this[path[0]]?.Walk(path.Skip()) ?? Enumerable.Empty<(RestPath.Match, T)>();
        }
    }
}
