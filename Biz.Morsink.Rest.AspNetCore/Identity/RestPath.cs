using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using QueryDict = System.Collections.Immutable.ImmutableSortedDictionary<string, Biz.Morsink.Rest.AspNetCore.RestPath.Query.Values>;
using static Biz.Morsink.Rest.AspNetCore.Utils.Utilities;
namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// This class represents a Rest path.
    /// It may contain wildcard parts "*".
    /// </summary>
    public struct RestPath : IEquatable<RestPath>
    {
        /// <summary>
        /// Enum to indicate whether a RestPath can be considered Local to the current server or Remote.
        /// </summary>
        public enum PathType { Local, Remote }
        /// <summary>
        /// Represents a single segment of a RestPath.
        /// </summary>
        public struct Segment : IEquatable<Segment>
        {
            private enum ContentType
            {
                Wildcard = 0,
                Content = 1,
                ComponentContent = 2
            }
            private readonly ContentType contentType;
            private Segment(string content, ContentType contentType = ContentType.Content)
            {
                Content = content;
                this.contentType = contentType;
            }

            /// <summary>
            /// Gets the content of the segment.
            /// </summary>
            public string Content { get; }
            /// <summary>
            /// True if this segment is a wildcard.
            /// </summary>
            public bool IsWildcard => contentType == ContentType.Wildcard;
            /// <summary>
            /// True if the segments carries a (possibly empty) component value.
            /// This means it is either a wildcard or an fixed value carrying an empty component.
            /// </summary>
            public bool IsComponent => contentType != ContentType.Content;
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
                => str == "*"
                    ? new Segment()
                    : str.EndsWith("+")
                        ? new Segment(UriDecode(str.Substring(0, str.Length - 1)), ContentType.ComponentContent)
                        : new Segment(UriDecode(str));
            /// <summary>
            /// Gets a Wildcard segment
            /// </summary>
            public static Segment Wildcard => default(Segment);

            public override string ToString()
                => IsWildcard ? "*" : (UriEncode(Content) + (IsComponent ? "+" : ""));

            public override int GetHashCode()
                => contentType == ContentType.Wildcard ? 0 : Content.GetHashCode();
            public override bool Equals(object obj)
                => obj is Segment && Equals((Segment)obj);
            public bool Equals(Segment other)
                => (contentType == other.contentType || contentType!=ContentType.Wildcard && other.contentType!=ContentType.Wildcard) 
                    && (IsWildcard || string.Equals(Content, other.Content));
            /// <summary>
            /// Determines whether the segments 'matches' the other.
            /// </summary>
            /// <param name="other">The segment to match.</param>
            /// <returns>True if the segments 'match'.</returns>
            public bool Matches(Segment other)
                => IsWildcard || other.IsWildcard || Equals(other);

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

                /// <summary>
                /// Gets the number of values in this entry.
                /// </summary>
                public int Count => value == null ? values == null ? 0 : values.Count : 1;

                /// <summary>
                /// Indexes into this collection of values.
                /// </summary>
                /// <param name="index">The index of the value</param>
                /// <returns>The value belonging to the specified index.</returns>
                public string this[int index]
                    => value == null
                        ? values == null
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
            public static Query Wildcard(Type[] wildcardTypes = null) => new Query(true, wildcardTypes);
            /// <summary>
            /// Creates an absent querystring.
            /// </summary>
            public static Query None => default(Query);
            /// <summary>
            /// Creates a Query with the given key-value pairs.
            /// </summary>
            /// <param name="pairs">Key value mappings for the query string.</param>
            /// <returns>A Query struct.</returns>
            public static Query FromPairs(IEnumerable<KeyValuePair<string, string>> pairs)
            {
                var b = QueryDict.Empty.ToBuilder();
                foreach (var x in pairs.GroupBy(p => p.Key, p => p.Value))
                    b.Add(x.Key, new Values(x));
                return new Query(b.ToImmutable());
            }
            /// <summary>
            /// Parses a query string *without leading '?') into a Query object.
            /// </summary>
            /// <param name="queryString">The to be parsed query string </param>
            /// <returns>A Query object corresponding to the specified query string.</returns>
            public static Query Parse(string queryString)
            {
                if (queryString == "*")
                    return Wildcard();
                else if (queryString == "")
                    return Empty;
                else if (queryString == null)
                    return None;
                else
                {
                    var kvps = from part in queryString.Split('&', ';')
                               let eqidx = part.IndexOf('=')
                               let key = eqidx >= 0 ? part.Substring(0, eqidx) : part
                               let val = eqidx >= 0 ? part.Substring(eqidx + 1) : null
                               select new KeyValuePair<string, string>(UriDecode(key), UriDecode(val));
                    return FromPairs(kvps);
                }
            }

            private readonly bool wildcard;
            private readonly Type[] wildcardTypes;
            private readonly QueryDict values;
            private Query(QueryDict values)
            {
                this.values = values;
                wildcard = false;
                wildcardTypes = null;
            }
            private Query(bool wildcard, Type[] wildcardTypes)
            {
                this.wildcardTypes = wildcardTypes;
                this.wildcard = wildcard;
                values = null;
            }
            /// <summary>
            /// True if this Query represents a global wildcard.
            /// </summary>
            public bool IsWildcard => wildcard;
            /// <summary>
            /// Gets a Type that matches the structure of the expected wildcard.
            /// </summary>
            public Type[] WildcardTypes => wildcard ? wildcardTypes : null;
            /// <summary>
            /// True if this Query represents the absence of a Query string.
            /// </summary>
            public bool IsNone => values == null && !wildcard;
            /// <summary>
            /// Adds a key-value mapping to the query string.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            /// <returns>A new Query struct containing the mapping.</returns>
            public Query Add(string key, string value)
            {
                if (IsNone || IsWildcard)
                    return this;
                else
                    return new Query(values.ContainsKey(key) ? values.SetItem(key, values[key].Add(value)) : values.SetItem(key, new Values(value)));
            }
            /// <summary>
            /// Adds key-value mappings to the query string.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="values">The values.</param>
            /// <returns>A new Query struct containing the mappings.</returns>
            public Query Add(string key, params string[] values)
            {
                if (IsNone || IsWildcard)
                    return this;
                else
                    return new Query(this.values.ContainsKey(key) ? this.values.SetItem(key, this.values[key].AddRange(values)) : this.values.SetItem(key, new Values(values)));
            }
            /// <summary>
            /// Applies a wildcard structure type to the query string, if it is a wildcard pattern.
            /// </summary>
            /// <param name="wildcardTypes">The structural type.</param>
            /// <returns>A Query with the type applied if it is a wildcard, <i>this</i> otherwise.</returns>
            public Query WithWildcardTypes(Type[] wildcardTypes)
                => IsWildcard ? new Query(true, wildcardTypes) : this;
            /// <summary>
            /// Gets the Values corresponding to some key.
            /// </summary>
            /// <param name="key">The key to get the Values for.</param>
            /// <returns>A Values struct containing the values belonging to the key.</returns>
            public Values this[string key] => values.TryGetValue(key, out var vals) ? vals : default(Values);

            /// <summary>
            /// Determines if a certain key is present in the query string.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <returns>True if the key is present in the Query.</returns>
            public bool ContainsKey(string key)
                => this[key].Count > 0;
            /// <summary>
            /// Tries to get the Values corresponding to some key.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The Values corresponding to the key.</param>
            /// <returns>True if the key was found.</returns>
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

            /// <summary>
            /// Gets the number of distinct keys in the Query.
            /// </summary>
            public int Count => values.Count;
            /// <summary>
            /// Converts the this Query object into a URI-suffix. 
            /// This includes a question mark if this Query is not 'None' (e.g. when it has value or it is Empty). 
            /// </summary>
            /// <returns>An URI suffix for this Query.</returns>
            public string ToUriSuffix()
                => IsNone ? "" : $"?{this}";
            public override string ToString()
            {
                if (IsNone)
                    return "";
                else if (IsWildcard)
                    return "*";
                else
                    return string.Join("&", values.SelectMany(kvp => kvp.Value.Select(val => $"{UriEncode(kvp.Key)}={UriEncode(val)}")));
            }

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
            public Match(RestPath path, IEnumerable<string> wildcardSegments, Query wildcardQuery)
            {
                Path = path;
                SegmentValues = wildcardSegments.ToArray();
                Query = wildcardQuery;
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
            /// <summary>
            /// Gets the matched query string.
            /// </summary>
            public Query Query { get; }
            /// <summary>
            /// Gets a dynamic part of a RestPath Match.
            /// </summary>
            /// <param name="index">The index of the corresponding wildcard.</param>
            /// <returns>The value for the indexed wildcard.</returns>
            public object this[int index] =>
                index == SegmentValues.Count
                    ? Query.IsNone
                        ? throw new ArgumentOutOfRangeException()
                        : (object)Query.ToDictionary(x => x.Key, x => x.Value[0])
                    : SegmentValues[index];
            /// <summary>
            /// Converts all the wildcard match values into an object array.
            /// </summary>
            /// <returns>An array of all the wildcard match values.</returns>
            public object[] ToArray()
                => SegmentValues.Cast<object>()
                    .Concat(Query.IsNone
                        ? Enumerable.Empty<object>()
                        : new[] { Query.ToDictionary(x => x.Key, x => x.Value[0]) })
                    .ToArray();
        }
        /// <summary>
        /// Parses a string into a RestPath instance.
        /// </summary>
        /// <param name="pathString">The path string to parse.</param>
        /// <param name="forType">The entity type the path belongs to.</param>
        /// <returns>A parsed RestPath instance.</returns>
        public static RestPath Parse(string pathString)
        {
            if (pathString.StartsWith("http://") || pathString.StartsWith("https://"))
            {
                var start = pathString.IndexOf('/', pathString.IndexOf(':') + 3);
                var pathBase = pathString.Substring(0, start);
                var (segments, query) = parseLocal(pathString.Substring(start));
                return new RestPath(pathBase, segments, query);
            }
            else
            {
                var (segments, query) = parseLocal(pathString);
                return new RestPath(segments, query);
            }

            (IEnumerable<Segment>, Query) parseLocal(string str)
            {
                var qidx = str.IndexOf('?');
                if (qidx >= 0)
                    return (makeSegments(str.Substring(0, qidx)), Query.Parse(str.Substring(qidx + 1)));
                else
                    return (makeSegments(str), Query.None);
            }
            IEnumerable<Segment> makeSegments(string path)
            {
                if (path == "/")
                    return Enumerable.Empty<Segment>();
                else
                    return path.Split('/').Select(Segment.Escaped).Skip(1);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="segments">All the path-parts of the path.</param>
        /// <param name="query">The query string part of the path.</param>
        /// <param name="forType">The entity type the path belongs to.</param>
        public RestPath(IEnumerable<Segment> segments, Query query)
            : this(null, segments, query)
        { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pathBase">The base path for this path.</param>
        /// <param name="segments">All the path-parts of the path.</param>
        /// <param name="query">The query string part of the path.</param>
        /// <param name="forType">The entity type the path belongs to.</param>
        public RestPath(string pathBase, IEnumerable<Segment> segments, Query query)
        {
            this.pathBase = pathBase;
            this.segments = segments.ToArray();
            this.query = query;
            skip = 0;
        }
        private RestPath(string pathBase, Segment[] segments, Query query, int skip)
        {
            this.pathBase = pathBase;
            this.segments = segments;
            this.query = query;
            this.skip = skip;
        }
        private readonly Segment[] segments;
        private readonly Query query;
        private readonly int skip;
        private readonly string pathBase;

        /// <summary>
        /// Gets the number of path parts in this RestPath.
        /// </summary>
        public int Count => segments.Length - skip;
        /// <summary>
        /// The base path for this path.
        /// </summary>
        public string PathBase => pathBase;
        /// <summary>
        /// Gets whether the path is a Local or Remote one.
        /// </summary>
        public PathType Location => pathBase == null ? PathType.Local : PathType.Remote;
        /// <summary>
        /// True if this is a local path.
        /// </summary>
        public bool IsLocal => pathBase == null;
        /// <summary>
        /// True if this is a remote path.
        /// </summary>
        public bool IsRemote => pathBase != null;
        internal int SegmentArity => segments.Where(s => s.IsComponent).Count();
        /// <summary>
        /// Gets the number of wildcard parts in this RestPath.
        /// </summary>
        public int Arity => SegmentArity + (query.IsWildcard ? 1 : 0);
        /// <summary>
        /// Gets the Query object corresponding to the RestPath's query string.
        /// </summary>
        public Query QueryString => query;

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
            => new RestPath(pathBase, segments, query, skip + num);
        /// <summary>
        /// Tries to match another Path to this one.
        /// </summary>
        /// <param name="other">The Path to match.</param>
        /// <returns>A Match instance containing the match results.</returns>
        public Match MatchPath(RestPath other, string localPathBase = null)
        {
            if (Count != other.Count
                || !string.Equals(pathBase ?? localPathBase, other.pathBase ?? localPathBase))
                return default(Match);
            var result = new List<string>();
            for (int i = 0; i < Count; i++)
            {
                if (this[i].IsWildcard)
                    result.Add(other[i].Content);
                else if (this[i].IsComponent)
                    result.Add("");
                else if (!this[i].Equals(other[i]))
                    return default(Match);
            }

            return new Match(this, result, query.IsWildcard ? other.query.IsNone ? Query.Empty : other.query : Query.None);
        }
        /// <summary>
        /// Gets the full RestPath this path was constructed from.
        /// </summary>
        /// <returns>A RestPath.</returns>
        public RestPath GetFullPath()
            => new RestPath(pathBase, segments, query, 0);

        private IEnumerable<Segment> fillHelper(IEnumerable<string> stars)
        {
            var s = stars.ToArray();
            int n = 0;
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].IsWildcard)
                    yield return Segment.Unescaped(s[n++]);
                else if (segments[i].IsComponent)
                {
                    yield return Segment.Unescaped(segments[i].Content);
                    n++;
                }
                else
                    yield return segments[i];
            }
        }
        /// <summary>
        /// Constructs a new Path by assigning values to the wildcards in the Path.
        /// </summary>
        /// <param name="wildcards">The values for the wildcards</param>
        /// <returns>A new Path</returns>
        public RestPath FillWildcards(IEnumerable<string> wildcards, Query query = default(Query))
            => new RestPath(fillHelper(wildcards), this.query.IsWildcard ? query : this.query);
        /// <summary>
        /// Gets a string representation for the Path.
        /// </summary>
        public string PathString => string.Concat(pathBase, "/" + string.Join("/", segments), query.ToUriSuffix());
        /// <summary>
        /// Turn the path into a remote path. Throws if the path is already remote.
        /// </summary>
        /// <param name="pathBase">The base path of the remote server.</param>
        /// <returns>A Remote RestPath.</returns>
        public RestPath ToRemote(string pathBase)
            => IsLocal ? new RestPath(pathBase, segments, query, skip) : throw new InvalidOperationException("Path is already remote.");
        /// <summary>
        /// Turn the path into a local path. 'Forgets' its PathBase.
        /// </summary>
        /// <returns>A Local RestPath.</returns>
        public RestPath ToLocal()
            => IsLocal ? this : new RestPath(null, segments, query, skip);
        /// <summary>
        /// Creates a new RestPath with the specified query string.
        /// </summary>
        /// <param name="q">The new query string.</param>
        /// <returns>A new RestPath with a new query string.</returns>
        public RestPath WithQuery(Query q)
            => new RestPath(pathBase, segments, q, skip);
        /// <summary>
        /// Creates a new RestPath with a manipulated query string.
        /// </summary>
        /// <param name="q">The query string manipulator.</param>
        /// <returns>A new RestPath with a new query string.</returns>
        public RestPath WithQuery(Func<Query, Query> q)
            => new RestPath(pathBase, segments, q(query), skip);

        public bool Equals(RestPath other)
            => Location == other.Location
            && PathBase == other.PathBase
            && segments.Length == other.segments.Length
            && segments.Zip(other.segments, (x, y) => x.Equals(y)).All(x => x);

        public override bool Equals(object o)
            => o is RestPath other ? Equals(other) : false;
        public override int GetHashCode()
        {
            int hc = typeof(RestPath).GetHashCode();
            hc = (hc << 3) + ((hc >> 29) & 0x7);
            hc ^= PathBase?.GetHashCode() ?? typeof(string).GetHashCode();
            for (int i = 0; i < segments.Length; i++)
            {
                hc = (hc << 3) + ((hc >> 29) & 0x7);
                hc ^= segments[i].GetHashCode();
            }
            return hc;
        }
        public override string ToString()
            => $"{PathBase}/{string.Join("/", segments)}{query.ToUriSuffix()}";
    }
}
