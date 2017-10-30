using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Biz.Morsink.Rest.Utils;
using System;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A (untyped) collection class for Rest parameters.
    /// </summary>
    public class RestParameterCollection : IEquatable<RestParameterCollection>
    {
        private static KeyValuePair<string, string>[] EMPTY = new KeyValuePair<string, string>[0];
        /// <summary>
        /// Gets an empty collection of Rest parameters.
        /// </summary>
        public static RestParameterCollection Empty { get; } = new RestParameterCollection(EMPTY);
        /// <summary>
        /// Creates a collection based on a collection of key value mappings.
        /// </summary>
        /// <param name="mappings">The Rest parameter value mappings.</param>
        /// <returns>A RestParameterCollection.</returns>
        public static RestParameterCollection Create(IEnumerable<KeyValuePair<string, string>> mappings)
            => mappings == null
                ? Empty
                : mappings is IReadOnlyCollection<KeyValuePair<string, string>> collection
                    ? Create(collection)
                    : new RestParameterCollection(mappings.ToArray());
        /// <summary>
        /// Creates a collection based on a collection of key value mappings.
        /// </summary>
        /// <param name="mappings">The Rest parameter value mappings.</param>
        /// <returns>A RestParameterCollection.</returns>
        public static RestParameterCollection Create(IReadOnlyCollection<KeyValuePair<string, string>> mappings)
            => mappings == null
                ? Empty
                : mappings is IReadOnlyList<KeyValuePair<string, string>> list
                    ? Create(list)
                    : mappings.Count == 0
                        ? Empty
                        : new RestParameterCollection(mappings.ToArray());
        /// <summary>
        /// Creates a collection based on a collection of key value mappings.
        /// </summary>
        /// <param name="mappings">The Rest parameter value mappings.</param>
        /// <returns>A RestParameterCollection.</returns>
        public static RestParameterCollection Create(IReadOnlyList<KeyValuePair<string, string>> mappings)
            => mappings == null
                ? Empty
                : mappings.Count == 0
                    ? Empty
                    : new RestParameterCollection(mappings.ToArray());
        /// <summary>
        /// Creates a collection based on a collection of key value mappings.
        /// </summary>
        /// <param name="mappings">The Rest parameter value mappings.</param>
        /// <returns>A RestParameterCollection.</returns>
        public static RestParameterCollection Create(IEnumerable<(string, string)> mappings)
            => mappings == null
                ? Empty
                : mappings is IReadOnlyCollection<(string, string)> collection
                    ? Create(collection)
                    : Create(mappings.Select(m => new KeyValuePair<string, string>(m.Item1, m.Item2)));
        /// <summary>
        /// Creates a collection based on a collection of key value mappings.
        /// </summary>
        /// <param name="mappings">The Rest parameter value mappings.</param>
        /// <returns>A RestParameterCollection.</returns>
        public static RestParameterCollection Create(IReadOnlyCollection<(string, string)> mappings)
            => mappings == null
                ? Empty
                : mappings is IReadOnlyList<(string, string)> list
                    ? Create(list)
                    : Create(mappings.Select(m => new KeyValuePair<string, string>(m.Item1, m.Item2)));
        /// <summary>
        /// Creates a collection based on a collection of key value mappings.
        /// </summary>
        /// <param name="mappings">The Rest parameter value mappings.</param>
        /// <returns>A RestParameterCollection.</returns>
        public static RestParameterCollection Create(IReadOnlyList<(string, string)> mappings)
            => mappings == null
                ? Empty
                : Create(mappings.Select(m => new KeyValuePair<string, string>(m.Item1, m.Item2)));

        private KeyValuePair<string, string>[] parameters;
        private ILookup<string, string> lookup;
        private IReadOnlyDictionary<string, string> firstDict;
        private RestParameterCollection(KeyValuePair<string, string>[] parameters)
        {
            this.parameters = parameters;
        }

        /// <summary>
        /// Converts the parameter collection to an ILookup&lt;string,string&gt; instance.
        /// </summary>
        /// <returns></returns>
        public ILookup<string, string> AsLookup()
            => lookup = lookup ?? parameters.ToLookup(p => p.Key, p => p.Value);
        /// <summary>
        /// Converts the parameter collection to an IReadOnlyDictionary&lt;string,string&gt; instance.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, string> AsDictionary()
            => firstDict = firstDict ?? parameters.GroupBy(p => p.Key).ToImmutableDictionary(p => p.Key, p => p.First().Value);

        /// <summary>
        /// Implements value equality for the RestParameterCollection.
        /// </summary>
        /// <param name="obj">The RestParameterCollection to compare to for equality.</param>
        /// <returns>True if the parameter is equal to this.</returns>
        public override bool Equals(object obj)
            => Equals(obj as RestParameterCollection);
        public override int GetHashCode()
            => parameters
                .OrderBy(p => p.Key)
                .ThenBy(p => p.Value)
                .Select(p => p.Key.GetHashCode() ^ p.Value.GetHashCode())
                .Aggregate((x, y) => x ^ y);
        /// <summary>
        /// Implements value equality for the RestParameterCollection.
        /// </summary>
        /// <param name="other">The RestParameterCollection to compare to for equality.</param>
        /// <returns>True if the parameter is equal to this.</returns>
        public bool Equals(RestParameterCollection other)
            => other != null
            && parameters
                .OrderBy(p => p.Key)
                .ThenBy(p => p.Value)
                .SequenceEqual(
                    other.parameters
                    .OrderBy(p => p.Key)
                    .ThenBy(p => p.Value));
        /// <summary>
        /// Operator for equality on RestParameterCollections.
        /// </summary>
        /// <param name="left">The left-hand side of the equality comparison.</param>
        /// <param name="right">The right-hand side of the equality comparison.</param>
        /// <returns>True if the two RestParameterCollections are equal.</returns>
        public static bool operator ==(RestParameterCollection left, RestParameterCollection right)
            => ReferenceEquals(left, right)
            || !ReferenceEquals(left, null) && left.Equals(right);
        /// <summary>
        /// Operator for inequality on RestParameterCollections.
        /// </summary>
        /// <param name="left">The left-hand side of the inequality comparison.</param>
        /// <param name="right">The right-hand side of the inequality comparison.</param>
        /// <returns>True if the two RestParameterCollections are not equal.</returns>
        public static bool operator !=(RestParameterCollection left, RestParameterCollection right)
            => !(left == right);
    }
}