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
    public class RestPathMatchTree
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
        private readonly ILookup<RestPath.Segment, RestPath> lookup;
        private readonly ConcurrentDictionary<RestPath.Segment, RestPathMatchTree> tree;

        /// <summary>
        /// Constructs a matching tree based on a collection of paths
        /// </summary>
        public RestPathMatchTree(IEnumerable<RestPath> paths)
        {
            lookup = paths.Where(p => p.Count > 0).ToLookup(p => p[0], SegmentEqualityComparer.Instance);
            tree = new ConcurrentDictionary<RestPath.Segment, RestPathMatchTree>();
            Terminals = paths.Where(p => p.Count == 0).ToArray();
        }

        /// <summary>
        /// Gets all the terminal paths at the current location in the tree.
        /// </summary>
        public IReadOnlyList<RestPath> Terminals { get; }

        /// <summary>
        /// Tries to navigate the tree.
        /// </summary>
        public RestPathMatchTree this[RestPath.Segment segment]
        {
            get
            {
                if (tree.TryGetValue(segment, out var val))
                    return val;
                else
                {
                    var paths = lookup[segment];
                    if (paths.Any())
                        return tree.GetOrAdd(segment, _ => new RestPathMatchTree(paths.Select(p => p.Skip())));
                    else
                        return segment.IsWildcard ? null : this[RestPath.Segment.Wildcard];
                }
            }
        }
        /// <summary>
        /// Try to match a certain path to some path in the tree.
        /// </summary>
        public RestPath.Match Walk(RestPath path)
        {
            if (path.Count == 0)
                return Terminals.Select(p => p.GetFullPath().MatchPath(path.GetFullPath()))
                    .Where(m => m.IsSuccessful).FirstOrDefault();
            else
                return this[path[0]]?.Walk(path.Skip()) ?? default(RestPath.Match);
        }
    }
}
