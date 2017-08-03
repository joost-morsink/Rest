using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.RestServer.Identity
{
    /// <summary>
    /// IdentityProvider for a Rest service.
    /// </summary>
    public class RestIdentityProvider : AbstractIdentityProvider
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
                switch (AllTypes.Length)
                {
                    case 1:
                        return typeof(string);
                    case 2:
                        return typeof((string, string));
                    case 3:
                        return typeof((string, string, string));
                    case 4:
                        return typeof((string, string, string, string));
                    case 5:
                        return typeof((string, string, string, string, string));
                    default:
                        return null;
                }
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
                if (p.Arity != types.Length)
                    throw new ArgumentException("Number of wildcards does not match arity of identity value.");
                return new EntryBuilder(parent, allTypes, paths.Add((p, types)));
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
                        if (!converter.Convert(res.Result).TryTo(out string[] compValues) || compValues.Length != entry.AllTypes.Length)
                            throw new ArgumentException("The number of component values does not match the arity of the identity value.");
                        var bld = parent.GeneralBuilder
                            .AddRange(entry.AllTypes.Zip(compValues, (t, cv) => (t, cv.GetType(), (object)cv)));
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

        private Dictionary<Type, Entry> entries = new Dictionary<Type, Entry>();
        private Lazy<RestPathMatchTree> matchTree;

        private RestPathMatchTree GetMatchTree()
            => new RestPathMatchTree(entries.SelectMany(e => e.Value.Paths));
        public RestIdentityProvider()
        {
            matchTree = new Lazy<RestPathMatchTree>(GetMatchTree);
        }
        /// <summary>
        /// Creates an entry builder
        /// </summary>
        /// <param name="types">The entity types for the identity value's components</param>
        /// <returns>An EntryBuilder</returns>
        protected EntryBuilder BuildEntry(params Type[] types)
            => EntryBuilder.Create(this, types);

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

    }
}
