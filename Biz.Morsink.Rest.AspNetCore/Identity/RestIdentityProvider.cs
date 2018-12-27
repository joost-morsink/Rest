using Biz.Morsink.DataConvert;
using Biz.Morsink.DataConvert.Converters;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    using System.Linq.Expressions;
    using static Utilities;
    /// <summary>
    /// IdentityProvider for a Rest service.
    /// </summary>
    public class RestIdentityProvider : AbstractIdentityProvider, IRestIdentityProvider
    {
        #region Helper classes
        private struct UnderlyingTypeBuilder
        {
            private readonly ImmutableStack<Type> stack;

            public static UnderlyingTypeBuilder Empty => new UnderlyingTypeBuilder(ImmutableStack<Type>.Empty);
            private UnderlyingTypeBuilder(ImmutableStack<Type> stack)
            {
                this.stack = stack;
            }
            public UnderlyingTypeBuilder Add(Type type)
                => new UnderlyingTypeBuilder(stack.Push(type));
            public UnderlyingTypeBuilder AddString(int num = 1)
                => num > 0 ? Add(typeof(string)).AddString(num - 1) : this;
            public Type ToType()
            {
                var types = stack.Reverse().ToArray();
                if (types.Length == 1)
                    return types[0];
                else if (types.Length < 8)
                    return typeof(ValueTuple).GetTypeInfo().Assembly.GetType($"System.ValueTuple`{types.Length}").MakeGenericType(types);
                else
                    throw new ArgumentOutOfRangeException("Too many types.");
            }
        }

        private class Entry
        {
            public Entry(Type type, Type[] allTypes, Version version, params string[] paths)
                : this(type, allTypes, version, paths.Select(path => RestPath.Parse(path)))
            { }
            public Entry(Type type, Type[] allTypes, Version version, IEnumerable<RestPath> paths)
            {
                Type = type;
                AllTypes = allTypes;
                Paths = paths.ToArray();
                Version = version;
            }
            public Type Type { get; }
            public Type[] AllTypes { get; }
            public IReadOnlyList<RestPath> Paths { get; }
            public Version Version { get; }

            public RestPath PrimaryPath => Paths[0];

            public Type GetUnderlyingType()
            {
                var tb = UnderlyingTypeBuilder.Empty.AddString(PrimaryPath.SegmentArity);
                if (PrimaryPath.QueryString.IsWildcard)
                    tb = tb.Add(typeof(Dictionary<string, string>));
                return tb.ToType();
            }
        }
        /// <summary>
        /// A helper struct to facilitate building entries in a RestIdentityProvider
        /// </summary>
        protected struct EntryBuilder
        {
            /// <summary>
            /// Creates a new EntryBuilder.
            /// </summary>
            /// <param name="parent">The ParentIdentityProvider the entry will be added to.</param>
            /// <param name="allTypes">The resource types of the identity value's components.</param>
            /// <returns>An EntryBuilder.</returns>
            internal static EntryBuilder Create(RestIdentityProvider parent, params Type[] allTypes)
                => new EntryBuilder(parent, allTypes, ImmutableList<(RestPath, Type[])>.Empty, VERSION_ONE);
            private readonly RestIdentityProvider parent;
            private readonly Type[] allTypes;
            private readonly ImmutableList<(RestPath, Type[])> paths;
            private readonly Version version;

            private EntryBuilder(RestIdentityProvider parent, Type[] allTypes, ImmutableList<(RestPath, Type[])> paths, Version version)
            {
                this.parent = parent;
                this.allTypes = allTypes;
                this.paths = paths;
                this.version = version;
            }
            /// <summary>
            /// Add a path to the entry.
            /// </summary>
            /// <param name="path">The rest path to add to the entry.</param>
            /// <returns>A new EntryBuilder containing the specified path.</returns>
            public EntryBuilder WithPath(RestPath path)
                => WithPath(path, allTypes);
            /// <summary>
            /// Add a path to the entry.
            /// </summary>
            /// <param name="path">The rest path to add to the entry.</param>
            /// <param name="types">The resource types of the identity value's components.</param>
            /// <returns>A new EntryBuilder containing the specified path.</returns>
            public EntryBuilder WithPath(RestPath path, params Type[] types)
            {
                types = types.Length == 0 ? allTypes : types;
                if (path.Arity != types.Length)
                    throw new ArgumentException("Number of wildcards does not match arity of identity value.");
                return new EntryBuilder(parent, allTypes, paths.Add((path, types)), version);
            }
            /// <summary>
            /// Adds a path to the entry.
            /// </summary>
            /// <param name="path">The path to add to the entry.</param>
            /// <returns>A new EntryBuilder containing the specified path.</returns>
            public EntryBuilder WithPath(string path)
                => WithPath(path, allTypes);
            /// <summary>
            /// Adds a path to the entry, with possibly a capped arity.
            /// </summary>
            /// <param name="path">The path to add to the entry.</param>
            /// <param name="arity">The arity of wildcards in the path.</param>
            /// <returns>A new EntryBuilder containing the specified path.</returns>
            public EntryBuilder WithPath(string path, int arity)
                => WithPath(path, allTypes.Skip(allTypes.Length - arity).ToArray());
            /// <summary>
            /// Adds multiple paths to the entry.
            /// </summary>
            /// <param name="paths">The paths to add to the entry.</param>
            /// <returns>A new EntryBuilder containing the specified paths.</returns>
            public EntryBuilder WithPaths(params string[] paths)
            {
                var res = this;
                foreach (var path in paths)
                    res = res.WithPath(path);
                return res;
            }
            /// <summary>
            /// Adds a path to the entry with a specific list of resource types.
            /// </summary>
            /// <param name="path">The path to add to the entry.</param>
            /// <param name="types">The resource types of the identity value's components.</param>
            /// <returns>A new EntryBuilder containing the specified path.</returns>
            public EntryBuilder WithPath(string path, params Type[] types)
            {
                var p = RestPath.Parse(path);
                return WithPath(p, types);
            }
            /// <summary>
            /// Sets the path and query string parameters.
            /// </summary>
            /// <param name="path">The rest path for the entry.</param>
            /// <param name="queryTypes">An optional array of query string parameter types.</param>
            /// <returns>A new EntryBuilder containing the specified path and query string parameter types.</returns>
            public EntryBuilder WithPathAndQueryType(string path, params Type[] queryTypes)
            {
                var p = RestPath.Parse(path);
                if (!p.QueryString.IsWildcard)
                    throw new ArgumentException("Query string must be wildcard.");
                return WithPath(p.WithQuery(q => q.WithWildcardTypes(queryTypes)));
            }
            /// <summary>
            /// Sets the version.
            /// </summary>
            /// <param name="major">The major version number.</param>
            /// <returns>A new EntryBuilder with a new version set.</returns>
            public EntryBuilder WithVersion(int major)
                => new EntryBuilder(parent, allTypes, paths, major == 1 ? VERSION_ONE : new Version(major, 0));
            /// <summary>
            /// Sets the version.
            /// </summary>
            /// <param name="major">The major version number.</param>
            /// <param name="minor">The minor version number.</param>
            /// <returns>A new EntryBuilder with a new version set.</returns>
            public EntryBuilder WithVersion(int major, int minor)
                => new EntryBuilder(parent, allTypes, paths, new Version(major, minor));
            /// <summary>
            /// Sets the version.
            /// </summary>
            /// <param name="major">The major version number.</param>
            /// <param name="minor">The minor version number.</param>
            /// <param name="patch">The patch version number.</param>
            /// <returns>A new EntryBuilder with a new version set.</returns>
            public EntryBuilder WithVersion(int major, int minor, int patch)
                => new EntryBuilder(parent, allTypes, paths, new Version(major, minor, patch));
            /// <summary>
            /// Sets the version.
            /// </summary>
            /// <param name="version">The version number.</param>
            /// <returns>A new EntryBuilder with a new version set.</returns>
            public EntryBuilder WithVersion(Version version)
                => new EntryBuilder(parent, allTypes, paths, version);

            /// <summary>
            /// Adds the entry to the parent PathIdentityProvider this builder was created from.
            /// </summary>
            public void Add()
            {
                if (!paths.IsEmpty)
                {
                    var entry = new Entry(allTypes[allTypes.Length - 1], allTypes, version, paths.Select(t => t.Item1));
                    parent.entries.Add(allTypes[allTypes.Length - 1], entry);
                    foreach (var path in paths)
                        if (parent.paths.TryGetValue(path.Item1, out var types))
                            types.Add(entry);
                        else
                            parent.paths[path.Item1] = new List<Entry> { entry };
                    parent.matchTree = new Lazy<RestPathMatchTree<Entry>>(parent.GetMatchTree);
                }
            }

        }
        private class CreatorForObject : IIdentityCreator<object>
        {
            private readonly RestIdentityProvider parent;
            private readonly IDataConverter converter;

            public CreatorForObject(RestIdentityProvider parent)
            {
                this.parent = parent;
                converter = parent.GetConverter(typeof(object), true);
            }

            public IIdentity<object> Create<K>(K value)
            {
                if (converter.Convert(value).TryTo(out string res))
                    return new Identity<object, string>(parent, res);
                else
                    return null;
            }

            IIdentity IIdentityCreator.Create<K>(K value)
                => Create(value);
        }
        private class Creator<T, U> : IIdentityCreator<T>
        {
            private readonly RestIdentityProvider parent;
            private readonly IDataConverter converter;
            private readonly Entry entry;

            public Creator(RestIdentityProvider parent, Entry entry)
            {
                this.parent = parent;
                this.entry = entry;
                converter = parent.GetConverter(typeof(T), true);
            }

            public IIdentity<T> Create<K>(K value)
            {
                var res = converter.DoConversion<K, U>(value);
                if (res.IsSuccessful)
                {
                    if (this.entry.AllTypes.Length == 1)
                        return new Identity<T, U>(parent, res.Result);
                    else
                    {
                        if (!converter.Convert(res.Result).TryTo(out object[] compValues) || compValues.Length != entry.AllTypes.Length)
                            throw new ArgumentException("The number of component values does not match the arity of the identity value.");
                        var bld = parent.GeneralBuilder
                            .AddRange(entry.AllTypes.Zip(compValues, (t, cv) => (t, cv.GetType(), cv)));
                        return (IIdentity<T>)bld.Id();
                    }
                }
                else
                    return null;
            }

            IIdentity IIdentityCreator.Create<K>(K value)
                => Create(value);
        }
        private class DictionaryToStringConverter : IConverter
        {
            public static DictionaryToStringConverter Instance { get; } = new DictionaryToStringConverter();
            private DictionaryToStringConverter() { }

            public bool SupportsLambda => true;

            public bool CanConvert(Type from, Type to)
                => from == typeof(Dictionary<string, string>) && to == typeof(string);

            public Delegate Create(Type from, Type to)
                => CreateLambda(from, to).Compile();

            public LambdaExpression CreateLambda(Type from, Type to)
                => Lambda();
            public Expression<Func<Dictionary<string, string>, ConversionResult<string>>> Lambda()
                => dict => new ConversionResult<string>(string.Join("\t", dict.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}")));
        }
        #endregion
        private readonly DataConverter converter = Converters.CreatePipeline(regular: Converters.Regular.Concat(new IConverter[] {
            DictionaryToStringConverter.Instance,
            RecordConverter.ForDictionaries()
        }));
        private Dictionary<Type, Entry> entries = new Dictionary<Type, Entry>();
        private Dictionary<RestPath, List<Entry>> paths = new Dictionary<RestPath, List<Entry>>();
        private readonly string localPrefix;
        private Lazy<RestPathMatchTree<Entry>> matchTree;

        private RestPathMatchTree<Entry> GetMatchTree()
            => new RestPathMatchTree<Entry>(entries.SelectMany(e => e.Value.Paths.Select(p => (p, e.Value))), localPrefix);
        /// <summary>
        /// Constructor.
        /// </summary>
        public RestIdentityProvider(string localPrefix = null)
        {
            this.localPrefix = localPrefix;
            matchTree = new Lazy<RestPathMatchTree<Entry>>(GetMatchTree);
        }
        /// <summary>
        /// Creates an entry builder
        /// </summary>
        /// <param name="types">The resource types for the identity value's components</param>
        /// <returns>An EntryBuilder</returns>
        protected EntryBuilder BuildEntry(params Type[] types)
            => EntryBuilder.Create(this, types);

        /// <summary>
        /// Gets a data converter for converting underlying identity values.
        /// </summary>
        /// <param name="t">The type of object the identity value refers to.</param>
        /// <param name="incoming">Indicates if the converter should handle incoming or outgoing conversions.</param>
        /// <returns>An IDataConverter that is able to make conversions between different types of values.</returns>
        public override IDataConverter GetConverter(Type t, bool incoming) => converter;
        /// <summary>
        /// This method should return the type of the underlying value for a certain resource type.
        /// </summary>
        /// <param name="forType">The resource type.</param>
        /// <returns>The type of the underlying identity values.</returns>
        public override Type GetUnderlyingType(Type forType)
        {
            return entries.TryGetValue(forType, out var e)
                ? e.GetUnderlyingType()
                : null;
        }

        /// <summary>
        /// Gets an IIdentityCreator for the specified entity type.
        /// </summary>
        /// <param name="type">The entity type to get an identity creator for.</param>
        /// <returns>An IIdentityCreator instance for the specified entity type.</returns>
        protected override IIdentityCreator GetCreator(Type type)
        {
            return type == typeof(object)
                ? new CreatorForObject(this)
                : entries.TryGetValue(type, out var ent)
                    ? (IIdentityCreator)Activator.CreateInstance(typeof(Creator<,>)
                        .MakeGenericType(type, ent.GetUnderlyingType()), this, ent)
                    : null;
        }
        /// <summary>
        /// Gets an IIdentityCreator&lt;T&gt; for some T.
        /// </summary>
        /// <typeparam name="T">The entity type to get an identity creator for.</typeparam>
        /// <returns>An IIdentityCreator&lt;T&gt; instance.</returns>
        protected override IIdentityCreator<T> GetCreator<T>()
            => (IIdentityCreator<T>)GetCreator(typeof(T));

        /// <summary>
        /// Gets all related types and version for which these types implement the 'same' repository.
        /// </summary>
        /// <param name="type">The type to check for related versions.</param>
        /// <returns>A list of version type pairs.</returns>
        public virtual IEnumerable<(Version, Type)> GetSupportedVersions(Type type)
        {
            if (entries.TryGetValue(type, out var entry))
            {
                if (paths.TryGetValue(entry.PrimaryPath, out var entries))
                    return entries.Select(e => (e.Version, e.Type));
            }
            return new[] { (VERSION_ONE, type) };
        }

        /// <summary>
        /// Parses a path and matches versions of rest repositories to the parsed path.
        /// </summary>
        /// <param name="path">The path to parse.</param>
        /// <param name="prefixes">A container of curie prefixes.</param>
        /// <returns>A list of identity matches.</returns>
        public virtual IEnumerable<RestIdentityMatch> Match(string path, RestPrefixContainer prefixes = null)
        {
            prefixes = prefixes ?? Prefixes;
            if (path.StartsWith("[") && path.EndsWith("]"))
            {
                var colonIndex = path.IndexOf(':');
                if (colonIndex < 0)
                    throw new ArgumentException("Safe Compact URI is not in proper format.", nameof(path));
                var prefix = path.Substring(1, colonIndex - 1);
                path = path.Substring(colonIndex + 1, path.Length - colonIndex - 2);
                if (!prefixes.TryGetByAbbreviation(prefix, out var rpref))
                    throw new ArgumentException("Prefix not found.", nameof(path));
                path = rpref.Prefix + path;
            }
            var matches = matchTree.Value.Walk(RestPath.Parse(path));
            return from m in matches
                   where m.Item1.IsSuccessful
                   select new RestIdentityMatch(m.Item1, m.Item2.Type, m.Item2.AllTypes, null, m.Item2.Version);
        }
        /// <summary>
        /// Parses a path string into an IIdentity value.
        /// If a match is found, the IIdentity value is properly typed.
        /// </summary>
        /// <param name="path">The path to parse.</param>
        /// <param name="nullOnFailure">When there is not match found for the Path, this boolean indicates whether to return a null or an IIdentity&lt;object&gt;.</param>
        /// <returns>An IIdentity value for the path.</returns>
        public virtual IIdentity Parse(string path, bool nullOnFailure = false, RestPrefixContainer prefixes = null, VersionMatcher versionMatcher = default)
        {
            var matches = Match(path, prefixes);
            var match = versionMatcher.Match(matches.Select(m => (m.Version, m))).Item2;
            if (match.IsSuccessful)
            {
                if (match.Path.Arity == 1)
                    return Create(match.ForType, match.Match[0]);
                else
                    return Create(match.ForType, match.Match.ToArray());
            }
            else
                return nullOnFailure ? null : new Identity<object, string>(this, path);
        }
        /// <summary>
        /// Parses a rest path when type information is already known.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="path">The path to parse.</param>
        /// <param name="prefixes">A container of curie prefixes.</param>
        /// <returns>An identity value for the specified path.</returns>
        public virtual IIdentity<T> Parse<T>(string path, RestPrefixContainer prefixes = null)
        {
            var matches = Match(path, prefixes).Where(m => m.IsSuccessful && m.ForType == typeof(T));
            if (!matches.Any())
                return null;
            var match = matches.First();
            if (match.Path.Arity == 1)
                return (IIdentity<T>)Create(match.ForType, match.Match[0]);
            else
                return (IIdentity<T>)Create(match.ForType, match.Match.ToArray());
        }
        /// <summary>
        /// Parses a rest path when type information is already known.
        /// </summary>
        /// <param name="path">The path to parse.</param>
        /// <param name="specific">The entity type.</param>
        /// <param name="prefixes">A container of curie prefixes.</param>
        /// <returns>An identity value for the specified path.</returns>
        public virtual IIdentity Parse(string path, Type specific, RestPrefixContainer prefixes = null)
        {
            var matches = Match(path, prefixes).Where(m => m.IsSuccessful && m.ForType == specific);
            if (!matches.Any())
                return null;
            var match = matches.First();
            if (match.Path.Arity == 1)
                return Create(match.ForType, match.Match[0]);
            else
                return Create(match.ForType, match.Match.ToArray());

        }

        /// <summary>
        /// Converts any identity value for a known type into a general identity value with a pathstring as underlying value.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IIdentity<object> ToGeneralIdentity(IIdentity id)
        {
            if (id.Provider != this)
                return ToGeneralIdentity(Translate(id));
            if (id.ForType == typeof(object))
                return (IIdentity<object>)id;

            var converter = GetConverter(id.ForType, false);
            var res = createIdentity();
            if (res?.Value.IsLocal == true)
                res = new Identity<object, RestPath>(this, res.Value.ToRemote(localPrefix));
            return res;

            Identity<object, RestPath> createIdentity()
            {
                if (entries.TryGetValue(id.ForType, out var entry))
                {
                    var queryString = id.ComponentValue as IEnumerable<KeyValuePair<string, string>>;
                    if (id.Arity == 1)
                    {
                        if (queryString != null)
                            return new Identity<object, RestPath>(
                                this,
                                entry.PrimaryPath.FillWildcards(Enumerable.Empty<string>(),
                                    RestPath.Query.FromPairs(queryString)));
                        else
                            return new Identity<object, RestPath>(this, entry.PrimaryPath.FillWildcards(new[] { converter.Convert(id.Value).To<string>() }));
                    }
                    else
                    {
                        if (queryString != null)
                            return new Identity<object, RestPath>(
                                this,
                                entry.PrimaryPath.FillWildcards(componentValues(id), RestPath.Query.FromPairs(queryString)));
                        else
                            return new Identity<object, RestPath>(this, entry.PrimaryPath.FillWildcards(converter.Convert(id.Value).To<string[]>()));
                    }
                }
                else
                    return null;
            }
            IEnumerable<string> componentValues(IIdentity idval)
            {
                var st = ImmutableStack<IIdentity>.Empty;
                while (idval != null)
                {
                    idval = (idval as IMultiaryIdentity)?.Parent;
                    if (idval != null)
                        st = st.Push(idval);
                }
                foreach (var x in st)
                    yield return converter.Convert(x.ComponentValue).To("");
            }
        }
        /// <summary>
        /// Gets a collection of paths for some resource type.
        /// </summary>
        /// <param name="forType">The resource type to get the paths for.</param>
        /// <returns>A list of Rest paths.</returns>
        public virtual IReadOnlyList<RestPath> GetRestPaths(Type forType)
            => entries.TryGetValue(forType, out var entry) ? entry.Paths : new RestPath[0];
        /// <summary>
        /// Contains a default collection of curie prefixes for the identity provider.
        /// </summary>
        public RestPrefixContainer Prefixes { get; } = new RestPrefixContainer();
    }
}
