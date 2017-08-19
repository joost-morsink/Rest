﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest
{
    public class RestParameterCollection
    {
        private static KeyValuePair<string, string>[] EMPTY = new KeyValuePair<string, string>[0];
        public static RestParameterCollection Empty { get; } = new RestParameterCollection(EMPTY);
        public static RestParameterCollection Create(IEnumerable<KeyValuePair<string, string>> mappings)
            => mappings == null
                ? Empty
                : mappings is IReadOnlyCollection<KeyValuePair<string, string>> collection
                    ? Create(collection)
                    : new RestParameterCollection(mappings.ToArray());
        public static RestParameterCollection Create(IReadOnlyCollection<KeyValuePair<string, string>> mappings)
            => mappings == null
                ? Empty
                : mappings is IReadOnlyList<KeyValuePair<string, string>> list
                    ? Create(list)
                    : mappings.Count == 0
                        ? Empty
                        : new RestParameterCollection(mappings.ToArray());
        public static RestParameterCollection Create(IReadOnlyList<KeyValuePair<string, string>> mappings)
            => mappings == null
                ? Empty
                : mappings.Count == 0
                    ? Empty
                    : new RestParameterCollection(mappings.ToArray());
        public static RestParameterCollection Create(IEnumerable<(string, string)> mappings)
            => mappings == null
                ? Empty
                : mappings is IReadOnlyCollection<(string, string)> collection
                    ? Create(collection)
                    : Create(mappings.Select(m => new KeyValuePair<string, string>(m.Item1, m.Item2)));
        public static RestParameterCollection Create(IReadOnlyCollection<(string, string)> mappings)
            => mappings == null
                ? Empty
                : mappings is IReadOnlyList<(string, string)> list
                    ? Create(list)
                    : Create(mappings.Select(m => new KeyValuePair<string, string>(m.Item1, m.Item2)));
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

        public ILookup<string, string> AsLookup()
            => lookup = lookup ?? parameters.ToLookup(p => p.Key, p => p.Value);
        public IReadOnlyDictionary<string, string> AsDictionary()
            => firstDict = firstDict ?? parameters.GroupBy(p => p.Key).ToImmutableDictionary(p => p.Key, p => p.First().Value);
    }
}