using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// This struct will represent the query string.
        /// </summary>
        public struct Query
        {

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
        public int Arity =>  segments.Where(s => s.IsWildcard).Count();

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
                if (segments[i] .IsWildcard)
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
