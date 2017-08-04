using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using QueryDict = System.Collections.Immutable.ImmutableSortedDictionary<string, Biz.Morsink.RestServer.Identity.RestPath.Query.Values>;
namespace Biz.Morsink.RestServer.Identity
{
    /// <summary>
    /// This class represents a Rest path.
    /// It may contain wildcard parts "*".
    /// </summary>
    public struct RestPath
    {
        /// <summary>
        /// Represents a single segment of a RestPath.
        /// </summary>
        public struct Segment : IEquatable<Segment>
        {
            private readonly bool hasContent;
            private Segment(string content)
            {
                Content = content;
                hasContent = true;
            }
            /// <summary>
            /// Gets the content of the segment.
            /// </summary>
            public string Content { get; }
            /// <summary>
            /// True if this segment is a wildcard.
            /// </summary>
            public bool IsWildcard => !hasContent;
            /// <summary>
            /// Converts an unescaped string to a segment.
            /// </summary>
            /// <param name="str">The unescaped string.</param>
            /// <returns>A Segment.</returns>
            public static Segment Unescaped(string str)
                => new Segment(str);
            /// <summary>
            /// Converts an escaped string to a segment.
            /// </summary>
            /// <param name="str">The escaped string to parse.</param>
            /// <returns>A segment.</returns>
            public static Segment Escaped(string str)
                => str == "*" ? new Segment() : new Segment(UnescapeSegment(str));
            public static Segment Wildcard => default(Segment);

            public override string ToString()
                => IsWildcard ? "*" : EscapeSegment(Content);

            public override int GetHashCode()
                => hasContent ? Content.GetHashCode() : 0;
            public override bool Equals(object obj)
                => obj is Segment && Equals((Segment)obj);
            public bool Equals(Segment other)
                => IsWildcard == other.IsWildcard && (IsWildcard || string.Equals(Content, other.Content));
            /// <summary>
            /// Determines whether the segments 'matches' the other.
            /// </summary>
            /// <param name="other">The segment to match.</param>
            /// <returns>True if the segments 'match'.</returns>
            public bool Matches(Segment other)
                => IsWildcard || other.IsWildcard || Equals(other);
            #region Helper functions
            /// <summary>
            /// Unescape a string into proper content
            /// </summary>
            public static string UnescapeSegment(string segment)
            {
                var n = segment.Length;
                var sb = new StringBuilder();
                for (int i = 0; i < n; i++)
                {
                    if (segment[i] != '%')
                        sb.Append(segment[i]);
                    else if (segment[i + 1] == '%')
                        sb.Append(segment[++i]);
                    else
                    {
                        sb.Append((char)int.Parse(segment.Substring(i + 1, 2), System.Globalization.NumberStyles.HexNumber));
                        i += 2;
                    }
                }
                return sb.ToString();
            }
            /// <summary>
            /// Determines if escaping is needed on a string
            /// </summary>
            public static bool IsEscapingNeeded(string segment)
            {
                var n = segment.Length;
                for (int i = 0; i < n; i++)
                    if (!IsSafeCharacter(segment[i]))
                        return true;
                return false;
            }
            /// <summary>
            /// Escapes non-safe characters in a string
            /// </summary>
            public static string EscapeSegment(string segment)
            {
                if (!IsEscapingNeeded(segment))
                    return segment;
                var n = segment.Length;
                var sb = new StringBuilder();
                for (int i = 0; i < n; i++)
                {
                    var ch = segment[i];
                    if (IsSafeCharacter(ch))
                        sb.Append(ch);
                    else if (ch == '%')
                        sb.Append("%%");
                    else
                    {
                        sb.Append('%');
                        sb.Append(BitConverter.ToString(new[] { (byte)ch }));
                    }
                }
                return sb.ToString();
            }
            /// <summary>
            /// Determines if a character is safe
            /// </summary>
            public static bool IsSafeCharacter(char ch)
                => ch >= '0' && ch <= '9'
                || ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z'
                || ch == '-' || ch == '_' || ch == '~';
            #endregion
        }
        /// <summary>
        /// This struct represents the query string.
        /// </summary>
        public struct Query : IReadOnlyDictionary<string, Query.Values>
        {
            /// <summary>
            /// This struct represents a set of values that correspond to a key in the query string.
            /// </summary>
            public struct Values : IReadOnlyList<string>
            {
                private readonly string value;
                private readonly ImmutableList<string> values;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="value">A single value.</param>
                public Values(string value)
                {
                    this.value = value;
                    values = null;
                }
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="values">An immutable list of values.</param>
                public Values(ImmutableList<string> values)
                {
                    this.values = values;
                    value = null;
                }
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="values">A number of values.</param>
                public Values(IEnumerable<string> values)
                    : this(ImmutableList.Create(values.ToArray()))
                { }
                /// <summary>
                /// Adds a value to the collection.
                /// </summary>
                /// <param name="item">The value to add.</param>
                /// <returns>A new Values struct containing the new item.</returns>
                public Values Add(string item)
                    => value == null ?
                        values == null ? new Values(item)
                        : new Values(values.Add(item))
                    : new Values(ImmutableList.Create(value, item));
                /// <summary>
                /// Adds values to the collection.
                /// </summary>
                /// <param name="items">The values to add.</param>
                /// <returns>A new Values struct containing the new items.</returns>
                public Values AddRange(IEnumerable<string> items)
                    => items.Aggregate(this, (v, x) => v.Add(x));

                public IEnumerator<string> GetEnumerator()
                {
                    var n = Count;
                    for (int i = 0; i < n; i++)
                        yield return this[i];
                }

                IEnumerator IEnumerable.GetEnumerator()
                    => GetEnumerator();

                public int Count => value == null ? values == null ? 0 : values.Count : 1;

                public string this[int index] 
                    => value == null 
                        ? values ==null 
                            ? throw new ArgumentOutOfRangeException() 
                            : values[index] 
                        : index == 0 
                            ? value 
                            : throw new ArgumentOutOfRangeException();
            }
            /// <summary>
            /// Creates an empty Query that allows Values to be added.
            /// </summary>
            public static Query Empty => new Query(QueryDict.Empty);
            /// <summary>
            /// Creates a 'wildcard' querystring.
            /// </summary>
            public static Query Wildcard => new Query(true);
            /// <summary>
            /// Creates a Query with the given key-value pairs.
            /// </summary>
            /// <param name="pairs">Key value mappings for the query string.</param>
            /// <returns>A Query struct.</returns>
            public static Query FromPairs(IEnumerable<KeyValuePair<string,string>> pairs)
            {
                var b = QueryDict.Empty.ToBuilder();
                foreach (var x in pairs.GroupBy(p => p.Key, p => p.Value))
                    b.Add(x.Key, new Values(x));
                return new Query(b.ToImmutable());
            }

            private readonly bool wildcard;
            private readonly QueryDict values;
            private Query(QueryDict values)
            {
                this.values = values;
                wildcard = false;
            }
            private Query(bool wildcard)
            {
                this.wildcard = wildcard;
                values = null;
            }
            /// <summary>
            /// True if this Query represents a global wildcard.
            /// </summary>
            public bool IsWildcard => wildcard;
            /// <summary>
            /// True if this Query represents the absence of a Query string.
            /// </summary>
            public bool IsNone => values == null;
            /// <summary>
            /// Adds a key-value mapping to the query string.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            /// <returns>A new Query struct containing the mapping.</returns>
            public Query Add(string key, string value)
            {
                var _this = this;
                return Switch(
                    values => new Query(values.ContainsKey(key) ? values.SetItem(key, values[key].Add(value)) : values.SetItem(key, new Values(value))),
                    () => _this,
                    () => _this);
            }
            /// <summary>
            /// Adds key-value mappings to the query string.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="values">The values.</param>
            /// <returns>A new Query struct containing the mappings.</returns>
            public Query Add(string key, params string[] values)
            {
                var _this = this;
                return Switch(
                    vals => new Query(vals.ContainsKey(key) ? vals.SetItem(key, vals[key].AddRange(values)) : vals.SetItem(key, new Values(values))),
                    () => _this,
                    () => _this);
            }
            /// <summary>
            /// Gets the Values corresponding to some key.
            /// </summary>
            /// <param name="key">The key to get the Values for.</param>
            /// <returns>A Values struct containing the values belonging to the key.</returns>
            public Values this[string key] => values.TryGetValue(key, out var vals) ? vals : default(Values);
            private R Switch<R>(Func<QueryDict, R> values, Func<R> wildcard, Func<R> none)
            {
                if (IsNone)
                    return none();
                else if (IsWildcard)
                    return wildcard();
                else
                    return values(this.values);
            }

            public bool ContainsKey(string key)
                => this[key].Count > 0;

            public bool TryGetValue(string key, out Values value)
            {
                value = this[key];
                return value.Count > 0;
            }

            public IEnumerator<KeyValuePair<string, Values>> GetEnumerator()
                => values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            public IEnumerable<string> Keys => values.Keys;

            IEnumerable<Values> IReadOnlyDictionary<string, Values>.Values => values.Values;

            public int Count => values.Count;

        }
        /// <summary>
        /// Represents a match on a path.
        /// </summary>
        public struct Match
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="path">The matched path.</param>
            /// <param name="wildcardSegments">The wildcard matches. null on unsuccesful match.</param>
            public Match(RestPath path, IEnumerable<string> wildcardSegments)
            {
                Path = path;
                SegmentValues = wildcardSegments.ToArray();
            }
            /// <summary>
            /// Indicates if the match is successful.
            /// </summary>
            public bool IsSuccessful => SegmentValues != null;
            /// <summary>
            /// The path that was matched.
            /// </summary>
            public RestPath Path { get; }
            /// <summary>
            /// The content of the wildcard matches.
            /// </summary>
            public IReadOnlyList<string> SegmentValues { get; }
            public string this[int index] => SegmentValues[index];
        }
        /// <summary>
        /// Parses a string into a RestPath instance.
        /// </summary>
        /// <param name="pathString">The path string to parse.</param>
        /// <param name="forType">The entity type the path belongs to.</param>
        /// <returns>A parsed RestPath instance.</returns>
        public static RestPath Parse(string pathString, Type forType = null)
        {
            var qidx = pathString.IndexOf('?');

            return new RestPath(pathString.Split('/').Select(Segment.Escaped), forType);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="segments">All the parts of the path.</param>
        /// <param name="forType">The entity type the path belongs to.</param>
        public RestPath(IEnumerable<Segment> segments, Type forType)
        {
            this.segments = segments.ToArray();
            skip = 0;
            ForType = forType;
        }
        private RestPath(Segment[] segments, int skip, Type forType)
        {
            this.segments = segments;
            this.skip = skip;
            ForType = forType;
        }
        private readonly Segment[] segments;
        private readonly int skip;

        /// <summary>
        /// Gets the number of path parts in this RestPath.
        /// </summary>
        public int Count => segments.Length - skip;
        /// <summary>
        /// Gets the number of wildcard parts in this RestPath.
        /// </summary>
        public int Arity => segments.Where(s => s.IsWildcard).Count();

        /// <summary>
        /// Gets the entity type this RestPath is for.
        /// </summary>
        public Type ForType { get; }
        /// <summary>
        /// Gets a specific element of this RestPath.
        /// </summary>
        /// <param name="index">The index of the part.</param>
        /// <returns>A specific element of the Path.</returns>
        public Segment this[int index] => segments[skip + index];
        /// <summary>
        /// Constructs a new RestPath, based on skipping some of the first segments.
        /// </summary>
        /// <param name="num">The number of segments to skip.</param>
        /// <returns>A shorter RestPath.</returns>
        public RestPath Skip(int num = 1)
            => new RestPath(segments, skip + num, ForType);
        /// <summary>
        /// Tries to match another Path to this one.
        /// </summary>
        /// <param name="other">The Path to match.</param>
        /// <returns>A Match instance containing the match results.</returns>
        public Match MatchPath(RestPath other)
        {
            if (Count != other.Count)
                return default(Match);
            var result = new List<string>();
            for (int i = 0; i < Count; i++)
            {
                if (this[i].IsWildcard)
                    result.Add(other[i].Content);
                else if (!this[i].Equals(other[i]))
                    return default(Match);
            }
            return new Match(this, result);
        }
        /// <summary>
        /// Gets the full RestPath this path was constructed from.
        /// </summary>
        /// <returns>A RestPath.</returns>
        public RestPath GetFullPath()
            => new RestPath(segments, 0, ForType);

        private IEnumerable<Segment> fillHelper(IEnumerable<string> stars)
        {
            var s = stars.ToArray();
            int n = 0;
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].IsWildcard)
                    yield return Segment.Unescaped(s[n++]);
                else
                    yield return segments[i];
            }
        }
        /// <summary>
        /// Constructs a new Path by assigning values to the wildcards in the Path.
        /// </summary>
        /// <param name="wildcards">The values for the wildcards</param>
        /// <returns>A new Path</returns>
        public RestPath FillWildcards(IEnumerable<string> wildcards)
            => new RestPath(fillHelper(wildcards), ForType);
        /// <summary>
        /// Gets a string representation for the Path.
        /// </summary>
        public string PathString => string.Join("/", segments);
    }
}
