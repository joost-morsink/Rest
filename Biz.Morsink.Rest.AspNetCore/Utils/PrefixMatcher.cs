using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    /// <summary>
    /// This class facilitates prefix matching for strings. 
    /// Prefixes map in a dictionary-like structure to some value of type T.
    /// This class is 'immutable'.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    [DebuggerDisplay("{DebugDisplay}")]
    public abstract class PrefixMatcher<T>
    {
        /// <summary>
        /// Create a PrefixMatcher based on a collection of prefix value pairs.
        /// </summary>
        /// <param name="elements">A collection of prefix value pairs.</param>
        /// <returns>A PrefixMatcher object.</returns>
        public static PrefixMatcher<T> Make(IEnumerable<(string, T)> elements)
        {
            return elements.Aggregate(Empty, (pm, el) => pm.Add(el.Item1, el.Item2));
        }
        /// <summary>
        /// Create a PrefixMatcher based on a collection of prefix value pairs.
        /// </summary>
        /// <param name="elements">A collection of prefix value pairs.</param>
        /// <returns>A PrefixMatcher object.</returns>        
        public static PrefixMatcher<T> Make(IEnumerable<KeyValuePair<string, T>> elements)
        {
            return elements.Aggregate(Empty, (pm, el) => pm.Add(el.Key, el.Value));
        }
        /// <summary>
        /// Gets an empty PrefixMatcher.
        /// </summary>
        public static PrefixMatcher<T> Empty => _Empty.Instance;
        /// <summary>
        /// Adds an entry to the PrefixMatcher.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="value">The value.</param>
        /// <returns>A new PrefixMatcher containing the added mapping.</returns>
        public abstract PrefixMatcher<T> Add(string prefix, T value);
        protected abstract PrefixMatcher<T> Translate(int offset);
        /// <summary>
        /// Gets a collection of all prefix value mappings.
        /// </summary>
        /// <returns>A collection of all prefix value mappings in this PrefixMatcher.</returns>
        public abstract IEnumerable<(string, T)> GetPrefixMatches();

        protected abstract void BuildDebugString(StringBuilder sb, int indent);
        /// <summary>
        /// Tries to match a string to a prefix in the PrefixMatcher.
        /// </summary>
        /// <param name="str">The string to test for prefixes.</param>
        /// <param name="result">The value of the mapping if a match was found.</param>
        /// <returns>True if a match was found, false otherwise.</returns>
        public abstract bool TryMatch(string str, out T result);

        /// <summary>
        /// Gets a pretty-printed textual representation of the PrefixMatcher.
        /// </summary>
        public string DebugDisplay
        {
            get
            {
                var sb = new StringBuilder();
                BuildDebugString(sb, 0);
                return sb.ToString();
            }
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        private PrefixMatcher() { }
        /// <summary>
        /// This class represents the empty PrefixMatcher.
        /// </summary>
        private sealed class _Empty : PrefixMatcher<T>
        {
            /// <summary>
            /// Gets the singleton instance.
            /// </summary>
            public static _Empty Instance { get; } = new _Empty();
            /// <summary>
            /// Constructor.
            /// </summary>
            private _Empty() { }
            /// <summary>
            /// Adding a mapping to an empty PrefixMatcher gives a single Leaf entry.
            /// </summary>
            /// <param name="prefix">The prefix to register.</param>
            /// <param name="value">The corresponding value.</param>
            /// <returns>A new PrefixMatcher.</returns>
            public override PrefixMatcher<T> Add(string prefix, T value)
                => new Leaf(new StringSlice(prefix), value);
            /// <summary>
            /// Empty does not translate.
            /// </summary>
            /// <returns>this</returns>
            protected override PrefixMatcher<T> Translate(int offset)
                => this;
            /// <summary>
            /// Gets an empty colllection of mappings.
            /// </summary>
            public override IEnumerable<(string, T)> GetPrefixMatches()
                => Enumerable.Empty<(string, T)>();
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                sb.Append(' ', indent);
                sb.AppendLine("()");
            }
            /// <summary>
            /// An empty PrefixMatcher never matches.
            /// </summary>
            /// <returns>false</returns>
            public override bool TryMatch(string str, out T result)
            {
                result = default(T);
                return false;
            }
        }
        /// <summary>
        /// A Leaf represents a single prefix mapping, optionally offset because of shorter partial prefixes.
        /// </summary>
        private sealed class Leaf : PrefixMatcher<T>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="slice">The slice of the Leaf.</param>
            /// <param name="value">The value.</param>
            public Leaf(StringSlice slice, T value)
            {
                this.slice = slice;
                this.value = value;
            }
            private readonly StringSlice slice;
            private readonly T value;

            public override PrefixMatcher<T> Add(string str, T item)
            {
                var length = slice.SubstringMatchLength(new StringSlice(str, slice.Offset));

                if (length == 0)
                    return new Node(slice.Prefix)
                        .Add(slice.FullString, value).Add(str, item);
                else
                    return new Fixed(slice.Slice(0, length),
                        new Node(slice.Slice(length).Prefix).Add(slice.FullString, value).Add(str, item));
            }
            protected override PrefixMatcher<T> Translate(int offset)
                => new Leaf(slice.Translate(offset), value);
            public override IEnumerable<(string, T)> GetPrefixMatches()
            {
                yield return (slice.FullString, value);
            }
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                sb.Append(' ', indent);
                sb.Append(slice.Value);
                sb.Append(" = ");
                sb.AppendLine(value?.ToString());
            }
            /// <summary>
            /// A match is found if the parameter starts with the Leaf's Prefix.
            /// </summary>
            /// <returns>True if the parameter matches the Prefix.</returns>
            public override bool TryMatch(string str, out T result)
            {
                if (new StringSlice(str, slice.Offset).SubstringMatchLength(slice) == slice.Length)
                {
                    result = value;
                    return true;
                }
                else
                {
                    result = default(T);
                    return false;
                }
            }
        }
        /// <summary>
        /// The Fixed class represents a fixed range in the prefix, that must match.
        /// </summary>
        private sealed class Fixed : PrefixMatcher<T>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="slice">The slice for the fixed range.</param>
            /// <param name="next">A PrefixMatcher that should apply after the fixed range.</param>
            public Fixed(StringSlice slice, PrefixMatcher<T> next)
            {
                this.slice = slice;
                this.next = next;
            }
            private readonly StringSlice slice;
            private readonly PrefixMatcher<T> next;
            public override PrefixMatcher<T> Add(string str, T item)
            {
                var len = slice.SubstringMatchLength(new StringSlice(str, slice.Offset));

                if (len == slice.Length)
                {
                    return new Fixed(slice, next.Add(str, item));
                }
                else if (len > 0)
                {
                    return new Fixed(slice.Slice(0, len), next.Translate(len - slice.Length)).Add(str, item);
                }
                else
                    return new Node(slice.Prefix,
                        ImmutableDictionary<char, PrefixMatcher<T>>.Empty
                        .Add(slice[0], this.Translate(1)), false).Add(str, item);

            }
            protected override PrefixMatcher<T> Translate(int offset)
            {
                if (offset > slice.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (offset == 0)
                    return this;
                else if (offset < 0)
                    return new Fixed(slice.MoveLeftBoundary(offset), next);
                else if (offset == slice.Length)
                    return next;
                else
                    return new Fixed(slice.Slice(offset), next);
            }
            public override IEnumerable<(string, T)> GetPrefixMatches()
                => next.GetPrefixMatches();
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                sb.Append(' ', indent);
                sb.Append(slice);
                sb.AppendLine(" -> ");
                next.BuildDebugString(sb, indent + slice.Length + 4);
            }
            /// <summary>
            /// Checks the fixed range and if equal, delegates matching to the next PrefixMatcher.
            /// </summary>
            public override bool TryMatch(string str, out T result)
            {
                if (str.Length < slice.PrefixWithSlice.Length)
                {
                    result = default(T);
                    return false;
                }
                else if (new StringSlice(str, slice.Offset, slice.Length).SubstringMatchLength(slice) == slice.Length)
                    return next.TryMatch(str, out result);
                else
                {
                    result = default(T);
                    return false;
                }
            }
        }
        /// <summary>
        /// A Node represents a point where a branch of PrefixMatchers is chosen based on a single character.
        /// </summary>
        private sealed class Node : PrefixMatcher<T>
        {
            /// <summary>
            /// Constructor for an empty Node.
            /// </summary>
            /// <param name="slice">A prefix slice for the Node.</param>
            public Node(StringSlice slice)
            {
                this.slice = slice;
                routes = ImmutableDictionary<char, PrefixMatcher<T>>.Empty;
                hasValue = false;
                value = default(T);
            }
            /// <summary>
            /// Constructor for a Node.
            /// </summary>
            /// <param name="slice">A prefix slice for the Node.</param>
            /// <param name="routes">The routes for the Node.</param>
            /// <param name="hasValue">Indicates whether the Node has a value for the prefix.</param>
            /// <param name="value">The value of the Node for the prefix.</param>
            public Node(StringSlice slice, ImmutableDictionary<char, PrefixMatcher<T>> routes, bool hasValue, T value = default(T))
            {
                this.slice = slice;
                this.routes = routes;
                this.hasValue = hasValue;
                this.value = value;
            }
            private readonly bool hasValue;
            private readonly T value;
            private readonly StringSlice slice;

            private readonly ImmutableDictionary<char, PrefixMatcher<T>> routes;

            public override PrefixMatcher<T> Add(string str, T item)
            {
                if (!str.StartsWith(slice.Value))
                    throw new ArgumentOutOfRangeException(nameof(str));
                if (str.Length == slice.Length)
                    return new Node(slice, routes, true, item);
                else if (routes.TryGetValue(str[slice.Length], out var next))
                    return new Node(slice, routes.SetItem(str[slice.Length], next.Add(str, item)), hasValue, value);
                else
                    return new Node(slice, routes.Add(str[slice.Length], new Leaf(new StringSlice(str, slice.Length + 1), item)), hasValue, value);
            }
            protected override PrefixMatcher<T> Translate(int offset)
            {
                if (offset > 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (offset == 0)
                    return this;
                else
                    return new Fixed(slice.MoveLeftBoundary(slice.Length + offset), this);
                    //return new Fixed(new StringSlice(slice.FullString, slice.Length + offset, -offset), this);
            }

            public override IEnumerable<(string, T)> GetPrefixMatches()
            {
                return routes.Values.SelectMany(s => s.GetPrefixMatches());
            }
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                if (hasValue)
                {
                    sb.Append(' ', indent);
                    sb.Append(" () -> ");
                    sb.AppendLine(value?.ToString());
                }
                foreach (var kvp in routes)
                {
                    sb.Append(' ', indent + 1);
                    sb.Append(kvp.Key);
                    sb.AppendLine(" -> ");
                    kvp.Value.BuildDebugString(sb, indent + 6);
                }
            }
            /// <summary>
            /// Selects the proper 'next' PrefixMatcher to delegate evaluation to. 
            /// Failure might be caught by an optional Value registered on the Node.
            /// </summary>
            public override bool TryMatch(string str, out T result)
            {
                if (str.Length == slice.Length)
                {
                    result = value;
                    return hasValue;
                }
                else if (str.Length > slice.Length)
                {
                    if (routes.TryGetValue(str[slice.Length], out var next))
                    {
                        if (next.TryMatch(str, out result))
                            return true;
                        else
                        {
                            result = value;
                            return hasValue;
                        }
                    }
                    else
                    {
                        result = value;
                        return hasValue;
                    }
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(str));
            }
        }
    }
}
