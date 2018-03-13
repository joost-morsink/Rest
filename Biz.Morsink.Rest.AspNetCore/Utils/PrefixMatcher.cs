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
        private static int SubstringMatchLength(string full, string sub, int offset)
        {
            int i;
            for (i = 0; i < sub.Length && offset + i < full.Length; i++)
                if (full[offset + i] != sub[i])
                    break;
            return i;
        }
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
                => new Leaf(prefix, 0, value);
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
            /// <param name="prefix">The prefix.</param>
            /// <param name="offset">The offset.</param>
            /// <param name="value">The value.</param>
            public Leaf(string prefix, int offset, T value)
            {
                Prefix = prefix;
                Offset = offset;
                Value = value;
            }
            /// <summary>
            /// Gets the Leaf's Prefix.
            /// </summary>
            public string Prefix { get; }
            /// <summary>
            /// Gets the offset in the Prefix.
            /// </summary>
            public int Offset { get; }
            /// <summary>
            /// Gets the corresponding value.
            /// </summary>
            public T Value { get; }
            public override PrefixMatcher<T> Add(string str, T item)
            {
                var length = SubstringMatchLength(Prefix, str, 0) - Offset;
                if (length == 0)
                    return new Node(Prefix.Substring(0, Offset))
                        .Add(Prefix, Value).Add(str, item);
                else
                    return new Fixed(Prefix.Substring(0, Offset), Prefix.Substring(Offset, length),
                        new Node(Prefix.Substring(0, Offset + length)).Add(Prefix, Value).Add(str, item));
            }
            protected override PrefixMatcher<T> Translate(int offset)
                => new Leaf(Prefix, Math.Max(0, Math.Min(Prefix.Length, Offset + offset)), Value);
            public override IEnumerable<(string, T)> GetPrefixMatches()
            {
                yield return (Prefix, Value);
            }
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                sb.Append(' ', indent);
                sb.Append(Prefix.Substring(Offset));
                sb.Append(" = ");
                sb.AppendLine(Value?.ToString());
            }
            /// <summary>
            /// A match is found if the parameter starts with the Leaf's Prefix.
            /// </summary>
            /// <returns>True if the parameter matches the Prefix.</returns>
            public override bool TryMatch(string str, out T result)
            {
                if (SubstringMatchLength(Prefix, str, 0) == Prefix.Length)
                {
                    result = Value;
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
            /// <param name="prefix">The prefix for the fixed range.</param>
            /// <param name="fixedRange">The fixed range.</param>
            /// <param name="next">A PrefixMatcher that should apply after the fixed range.</param>
            public Fixed(string prefix, string fixedRange, PrefixMatcher<T> next)
            {
                FixedRange = fixedRange;
                Prefix = prefix;
                Next = next;
            }

            public int Offset => Prefix.Length;
            /// <summary>
            /// Gets the fixed range.
            /// </summary>
            public string FixedRange { get; }
            /// <summary>
            /// Gets the fixed range's prefix.
            /// </summary>
            public string Prefix { get; }
            /// <summary>
            /// Gets the next PrefixMatcher.
            /// </summary>
            public PrefixMatcher<T> Next { get; }
            public override PrefixMatcher<T> Add(string str, T item)
            {
                var len = SubstringMatchLength(str, FixedRange, Offset);
                if (len == FixedRange.Length)
                {
                    return new Fixed(Prefix, FixedRange, Next.Add(str, item));
                }
                else if (len > 0)
                {
                    return new Fixed(Prefix, FixedRange.Substring(0, len), Next.Translate(len - FixedRange.Length)).Add(str, item);
                }
                else
                    return new Node(Prefix,
                        ImmutableDictionary<char, PrefixMatcher<T>>.Empty
                        .Add(FixedRange[0], this.Translate(1)),false).Add(str, item);

            }
            protected override PrefixMatcher<T> Translate(int offset)
            {
                if (offset > FixedRange.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (offset == 0)
                    return this;
                else if (offset < 0)
                    return new Fixed(Prefix.Substring(0, Prefix.Length + offset), Prefix.Substring(Prefix.Length + offset) + FixedRange, Next);
                else if (offset == FixedRange.Length)
                    return Next;
                else
                    return new Fixed(Prefix + FixedRange.Substring(0, offset), FixedRange.Substring(offset), Next);
            }
            public override IEnumerable<(string, T)> GetPrefixMatches()
                => Next.GetPrefixMatches();
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                sb.Append(' ', indent);
                sb.Append(FixedRange);
                sb.AppendLine(" -> ");
                Next.BuildDebugString(sb, indent + FixedRange.Length + 4);
            }
            /// <summary>
            /// Checks the fixed range and if equal, delegates matching to the Next PrefixMatcher.
            /// </summary>
            public override bool TryMatch(string str, out T result)
            {
                if (SubstringMatchLength(str, FixedRange, Offset) == FixedRange.Length)
                    return Next.TryMatch(str, out result);
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
            /// <param name="prefix">The prefix for the Node.</param>
            public Node(string prefix)
            {
                Prefix = prefix;
                Routes = ImmutableDictionary<char, PrefixMatcher<T>>.Empty;
                hasValue = false;
                value = default(T);
            }
            /// <summary>
            /// Constructor for a Node.
            /// </summary>
            /// <param name="prefix">The prefix for the Node.</param>
            /// <param name="routes">The routes for the Node.</param>
            /// <param name="hasValue">Indicates whether the Node has a value for the prefix.</param>
            /// <param name="value">The value of the Node for the prefix.</param>
            public Node(string prefix, ImmutableDictionary<char, PrefixMatcher<T>> routes, bool hasValue, T value = default(T))
            {
                Prefix = prefix;
                Routes = routes;
                this.hasValue = hasValue;
                this.value = value;
            }
            private readonly bool hasValue;
            private readonly T value;
            /// <summary>
            /// True if the Node has a value for the prefix.
            /// </summary>
            public bool HasValue => hasValue;
            /// <summary>
            /// The Value for the prefix if HasValue == true.
            /// </summary>
            public T Value => value;
            public int Offset => Prefix.Length;
            /// <summary>
            /// Gets the Node's prefix.
            /// </summary>
            public string Prefix { get; }
            public ImmutableDictionary<char, PrefixMatcher<T>> Routes { get; }

            public override PrefixMatcher<T> Add(string str, T item)
            {
                if (!str.StartsWith(Prefix))
                    throw new ArgumentOutOfRangeException(nameof(str));
                if (str.Length == Offset)
                    return new Node(Prefix, Routes, true, item);
                else if (Routes.TryGetValue(str[Offset], out var next))
                    return new Node(Prefix, Routes.SetItem(str[Offset], next.Add(str, item)), HasValue, Value);
                else
                    return new Node(Prefix, Routes.Add(str[Offset], new Leaf(str, Offset + 1, item)), HasValue, Value);
            }
            protected override PrefixMatcher<T> Translate(int offset)
            {
                if (offset > 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (offset == 0)
                    return this;
                else
                    return new Fixed(Prefix.Substring(0, Prefix.Length + offset), Prefix.Substring(Prefix.Length + offset), this);
            }

            public override IEnumerable<(string, T)> GetPrefixMatches()
            {
                return Routes.Values.SelectMany(s => s.GetPrefixMatches());
            }
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                if (HasValue)
                {
                    sb.Append(' ', indent);
                    sb.Append(" () -> ");
                    sb.AppendLine(Value?.ToString());
                }
                foreach (var kvp in Routes)
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
                if (str.Length == Prefix.Length)
                {
                    result = Value;
                    return HasValue;
                }
                else if (str.Length > Prefix.Length)
                {
                    if (Routes.TryGetValue(str[Offset], out var next))
                    {
                        if (next.TryMatch(str, out result))
                            return true;
                        else
                        {
                            result = Value;
                            return HasValue;
                        }
                    }
                    else
                    {
                        result = Value;
                        return HasValue;
                    }
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(str));
            }
        }
    }
}
