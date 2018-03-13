using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    [DebuggerDisplay("{DebugDisplay}")]
    public abstract class PrefixMatcher<T>
    {
        public static int SubstringMatchLength(string full, string sub, int offset)
        {
            int i;
            for (i = 0; i < sub.Length && offset + i < full.Length; i++)
                if (full[offset + i] != sub[i])
                    break;
            return i;
        }
        public static PrefixMatcher<T> Make(IEnumerable<(string, T)> elements)
        {
            return elements.Aggregate(Empty, (pm, el) => pm.Add(el.Item1, el.Item2));
        }
        public static PrefixMatcher<T> Make(IEnumerable<KeyValuePair<string, T>> elements)
        {
            return elements.Aggregate(Empty, (pm, el) => pm.Add(el.Key, el.Value));
        }
        public static PrefixMatcher<T> Empty { get; } = new _Empty();
        public abstract PrefixMatcher<T> Add(string str, T item);
        protected abstract PrefixMatcher<T> Translate(int offset);
        public abstract IEnumerable<(string, T)> GetPrefixMatches();

        protected abstract void BuildDebugString(StringBuilder sb, int indent);
        public abstract bool TryMatch(string str, out T result);

        public string DebugDisplay
        {
            get
            {
                var sb = new StringBuilder();
                BuildDebugString(sb, 0);
                return sb.ToString();
            }
        }
        private PrefixMatcher() { }

        private sealed class _Empty : PrefixMatcher<T>
        {
            public _Empty() { }
            public override PrefixMatcher<T> Add(string str, T item)
                => new Leaf(str, 0, item);
            protected override PrefixMatcher<T> Translate(int offset)
                => this;
            public override IEnumerable<(string, T)> GetPrefixMatches()
                => Enumerable.Empty<(string, T)>();
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                sb.Append(' ', indent);
                sb.AppendLine("()");
            }
            public override bool TryMatch(string str, out T result)
            {
                result = default(T);
                return false;
            }
        }
        private sealed class Leaf : PrefixMatcher<T>
        {
            public Leaf(string str, int offset, T item)
            {
                String = str;
                Offset = offset;
                Value = item;
            }
            public string String { get; }
            public int Offset { get; }
            public T Value { get; }
            public override PrefixMatcher<T> Add(string str, T item)
            {
                var length = SubstringMatchLength(String, str, 0) - Offset;
                if (length == 0)
                    return new Node(String.Substring(0, Offset))
                        .Add(String, Value).Add(str, item);
                else
                    return new Fixed(String.Substring(0, Offset), String.Substring(Offset, length),
                        new Node(String.Substring(0, Offset + length)).Add(String, Value).Add(str, item));
            }
            protected override PrefixMatcher<T> Translate(int offset)
                => new Leaf(String, Math.Max(0, Math.Min(String.Length, Offset + offset)), Value);
            public override IEnumerable<(string, T)> GetPrefixMatches()
            {
                yield return (String, Value);
            }
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                sb.Append(' ', indent);
                sb.Append(String.Substring(Offset));
                sb.Append(" = ");
                sb.AppendLine(Value?.ToString());
            }
            public override bool TryMatch(string str, out T result)
            {
                if (SubstringMatchLength(String, str, 0) == String.Length)
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
        private sealed class Fixed : PrefixMatcher<T>
        {
            public Fixed(string prefix, string fix, PrefixMatcher<T> next)
            {
                Fix = fix;
                Prefix = prefix;
                Next = next;
            }

            public int Offset => Prefix.Length;
            public string Fix { get; }
            public string Prefix { get; }
            public PrefixMatcher<T> Next { get; }
            public override PrefixMatcher<T> Add(string str, T item)
            {
                var len = SubstringMatchLength(str, Fix, Offset);
                if (len == Fix.Length)
                {
                    return new Fixed(Prefix, Fix, Next.Add(str, item));
                }
                else if (len > 0)
                {
                    return new Fixed(Prefix, Fix.Substring(0, len), Next.Translate(len - Fix.Length)).Add(str, item);
                }
                else
                    return new Node(Prefix,
                        ImmutableDictionary<char, PrefixMatcher<T>>.Empty
                        .Add(Fix[0], this.Translate(1))).Add(str, item);

            }
            protected override PrefixMatcher<T> Translate(int offset)
            {
                if (offset > Fix.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (offset == 0)
                    return this;
                else if (offset < 0)
                    return new Fixed(Prefix.Substring(0, Prefix.Length + offset), Prefix.Substring(Prefix.Length + offset) + Fix, Next);
                else if (offset == Fix.Length)
                    return Next;
                else
                    return new Fixed(Prefix + Fix.Substring(0, offset), Fix.Substring(offset), Next);
            }
            public override IEnumerable<(string, T)> GetPrefixMatches()
                => Next.GetPrefixMatches();
            protected override void BuildDebugString(StringBuilder sb, int indent)
            {
                sb.Append(' ', indent);
                sb.Append(Fix);
                sb.AppendLine(" -> ");
                Next.BuildDebugString(sb, indent + Fix.Length + 4);
            }
            public override bool TryMatch(string str, out T result)
            {
                if (SubstringMatchLength(str, Fix, Offset) == Fix.Length)
                    return Next.TryMatch(str, out result);
                else
                {
                    result = default(T);
                    return false;
                }
            }
        }
        private sealed class Node : PrefixMatcher<T>
        {
            public Node(string prefix)
            {
                Prefix = prefix;
                Routes = ImmutableDictionary<char, PrefixMatcher<T>>.Empty;
                hasValue = false;
                value = default(T);
            }
            public Node(string prefix, ImmutableDictionary<char, PrefixMatcher<T>> routes)
            {
                Prefix = prefix;
                Routes = routes;
                hasValue = false;
                value = default(T);
            }
            public Node(string prefix, ImmutableDictionary<char, PrefixMatcher<T>> routes, bool hasValue, T value = default(T))
            {
                Prefix = prefix;
                Routes = routes;
                this.hasValue = hasValue;
                this.value = value;
            }
            private readonly bool hasValue;
            private readonly T value;
            public bool HasValue => hasValue;
            public T Value => value;

            public int Offset => Prefix.Length;
            public string Prefix { get; }
            public ImmutableDictionary<char, PrefixMatcher<T>> Routes;

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
