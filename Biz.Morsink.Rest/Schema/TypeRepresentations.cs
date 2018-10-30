using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// Default implementation of ITypeRepresentations.
    /// Caches requested type information.
    /// </summary>
    public class TypeRepresentations : ITypeRepresentations, ITypeRepresentation
    {
        private readonly IEnumerable<ITypeRepresentation> representations;
        private readonly ConcurrentDictionary<Type, (ITypeRepresentation, Type)> representationTypes;
        private readonly ConcurrentDictionary<Type, (ITypeRepresentation, Type)> representableTypes;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="representations">The representations underlying the instance.</param>
        public TypeRepresentations(IEnumerable<ITypeRepresentation> representations)
        {
            this.representations = representations;
            representationTypes = new ConcurrentDictionary<Type, (ITypeRepresentation, Type)>();
            representableTypes = new ConcurrentDictionary<Type, (ITypeRepresentation, Type)>();
        }
        /// <summary>
        /// Get all individual type representations.
        /// </summary>
        public IEnumerable<ITypeRepresentation> GetTypeRepresentations() => representations;
        /// <summary>
        /// Returns a single type representation based on all underlying representations.
        /// The returned representation should implement a default (identity) representation, meaning representation is a total function.
        /// </summary>
        public ITypeRepresentation AsTypeRepresentation() => this;

        private (ITypeRepresentation, T) Traverse<T>(Func<ITypeRepresentation, T> func, T def)
            where T : class
            => representations.Select(tr => (tr, func(tr))).Where(t => t.Item2 != null).Append((null, def)).First();

        object ITypeRepresentation.GetRepresentable(object rep, Type specific)
            => Traverse(tr => tr.GetRepresentable(rep, specific), rep).Item2;

        Type ITypeRepresentation.GetRepresentableType(Type type)
            => representableTypes.GetOrAdd(type, ty => Traverse(tr => tr.GetRepresentableType(ty), ty)).Item2;

        object ITypeRepresentation.GetRepresentation(object obj)
            => Traverse(tr => tr.GetRepresentation(obj), obj).Item2;

        Type ITypeRepresentation.GetRepresentationType(Type type)
            => representationTypes.GetOrAdd(type, ty => Traverse(tr => tr.GetRepresentationType(ty), ty)).Item2;

        bool ITypeRepresentation.IsRepresentable(Type type) => true;

        bool ITypeRepresentation.IsRepresentation(Type type) => true;

    }
}
