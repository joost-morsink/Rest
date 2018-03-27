using Biz.Morsink.DataConvert;
using Biz.Morsink.DataConvert.Converters;
using Biz.Morsink.Identity;
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
            public Entry(Type type, Type[] allTypes, params string[] paths)
                : this(type, allTypes, paths.Select(path => RestPath.Parse(path, type)))
            { }
            public Entry(Type type, Type[] allTypes, IEnumerable<RestPath> paths)
            {
                Type = type;
                AllTypes = allTypes;
                Paths = paths.ToArray();
            }
            public Type Type { get; }
            public Type[] AllTypes { get; }
            public IReadOnlyList<RestPath> Paths { get; }
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
            /// <param name="allTypes">The entity types of the identity value's components.</param>
            /// <returns>An EntryBuilder.</returns>
            internal static EntryBuilder Create(RestIdentityProvider parent, params Type[] allTypes)
                => new EntryBuilder(parent, allTypes, ImmutableList<(RestPath, Type[])>.Empty);
            private readonly RestIdentityProvider parent;
            private readonly Type[] allTypes;
            private readonly ImmutableList<(RestPath, Type[])> paths;

            private EntryBuilder(RestIdentityProvider parent, Type[] allTypes, ImmutableList<(RestPath, Type[])> paths)
            {
                this.parent = parent;
                this.allTypes = allTypes;
                this.paths = paths;
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
            /// <param name="types">The entity types of the identity value's components.</param>
            /// <returns>A new EntryBuilder containing the specified path.</returns>
            public EntryBuilder WithPath(RestPath path, params Type[] types)
            {
                types = types.Length == 0 ? allTypes : types;
                if (path.Arity != types.Length)
                    throw new ArgumentException("Number of wildcards does not match arity of identity value.");
                return new EntryBuilder(parent, allTypes, paths.Add((path, types)));
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
            /// Adds a path to the entry with a specific list of entity types.
            /// </summary>
            /// <param name="path">The path to add to the entry.</param>
            /// <param name="types">The entity types of the identity value's components.</param>
            /// <returns>A new EntryBuilder containing the specified path.</returns>
            public EntryBuilder WithPath(string path, params Type[] types)
            {
                var p = RestPath.Parse(path, allTypes[allTypes.Length - 1]);
                return WithPath(p, types);
            }
            public EntryBuilder WithPathAndQueryType(string path, Type queryType)
            {
                var p = RestPath.Parse(path, allTypes[allTypes.Length - 1]);
                if (!p.QueryString.IsWildcard)
                    throw new ArgumentException("Query string must be wildcard.");
                return WithPath(p.WithQuery(q => q.WithWildcardType(queryType)));
            }
            /// <summary>
            /// Adds the entry to the parent PathIdentityProvider this builder was created from.
            /// </summary>
            public void Add()
            {
                if (!paths.IsEmpty)
                {
                    parent.entries.Add(allTypes[allTypes.Length - 1], new Entry(allTypes[allTypes.Length - 1], allTypes, paths.Select(t => t.Item1)));
                    parent.matchTree = new Lazy<RestPathMatchTree>(parent.GetMatchTree);
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
        #endregion
        private DataConverter converter = Converters.CreatePipeline(regular: Converters.Regular.Concat(new IConverter[] {
            RecordConverter.ForDictionaries()
        }));
        private Dictionary<Type, Entry> entries = new Dictionary<Type, Entry>();
        private readonly string localPrefix;
        private Lazy<RestPathMatchTree> matchTree;

        private RestPathMatchTree GetMatchTree()
            => new RestPathMatchTree(entries.SelectMany(e => e.Value.Paths), localPrefix);
        /// <summary>
        /// Constructor.
        /// </summary>
        public RestIdentityProvider(string localPrefix = null)
        {
            this.localPrefix = localPrefix;
            matchTree = new Lazy<RestPathMatchTree>(GetMatchTree);
        }
        /// <summary>
        /// Creates an entry builder
        /// </summary>
        /// <param name="types">The entity types for the identity value's components</param>
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
        /// This method should return the type of the underlying value for a certain entity type.
        /// </summary>
        /// <param name="forType">The entity type.</param>
        /// <returns>The type of the underlying identity values.</returns>
        public override Type GetUnderlyingType(Type forType)
        {
            return entries.TryGetValue(forType, out var e)
                ? e.GetUnderlyingType()
                : null;
        }

        protected override IIdentityCreator GetCreator(Type type)
        {
            return type == typeof(object)
                ? new CreatorForObject(this)
                : entries.TryGetValue(type, out var ent)
                    ? (IIdentityCreator)Activator.CreateInstance(typeof(Creator<,>)
                        .MakeGenericType(type, ent.GetUnderlyingType()), this, ent)
                    : null;
        }

        protected override IIdentityCreator<T> GetCreator<T>()
            => (IIdentityCreator<T>)GetCreator(typeof(T));

        /// <summary>
        /// Parses a path string into an IIdentity value.
        /// If a match is found, the IIdentity value is properly typed.
        /// </summary>
        /// <param name="path">The path to parse.</param>
        /// <param name="nullOnFailure">When there is not match found for the Path, this boolean indicates whether to return a null or an IIdentity&lt;object&gt;.</param>
        /// <returns>An IIdentity value for the path.</returns>
        public virtual IIdentity Parse(string path, bool nullOnFailure = false, RestPrefixContainer prefixes = null)
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
            var match = matchTree.Value.Walk(RestPath.Parse(path));
            if (match.IsSuccessful)
            {
                if (match.Path.Arity == 1)
                    return Create(match.Path.ForType, match[0]);
                else
                    return Create(match.Path.ForType, match.ToArray());
            }
            else
                return nullOnFailure ? null : new Identity<object, string>(this, path);
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
        public virtual IReadOnlyList<RestPath> GetRestPaths(Type forType)
            => entries.TryGetValue(forType, out var entry) ? entry.Paths : new RestPath[0];

        public RestPrefixContainer Prefixes { get; } = new RestPrefixContainer();
    }
}
