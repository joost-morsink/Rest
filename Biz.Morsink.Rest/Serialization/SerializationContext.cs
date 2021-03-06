﻿using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// This class provides a contextual object for use in serialization.
    /// </summary>
    public abstract class SerializationContext<C>
        where C : SerializationContext<C>
    {
        /// <summary>
        /// Contains the identity provider.
        /// </summary>
        protected IIdentityProvider IdentityProvider { get; }
        /// <summary>
        /// Contains a dictonary of embeddings that can be referenced.
        /// </summary>
        protected ImmutableDictionary<IIdentity, Embedding> Embeddings { get; }
        /// <summary>
        /// Contains a stack of identities of containing objects to prevent circular serialization stack overflows.
        /// </summary>
        protected ImmutableStack<IIdentity> ParentChain { get; }

        /// <summary>
        /// Consructor.
        /// </summary>
        /// <param name="identityProvider"></param>
        /// <param name="previous"></param>
        /// <param name="embeddings"></param>
        /// <param name="parentChain"></param>
        protected SerializationContext(IIdentityProvider identityProvider, C previous, ImmutableDictionary<IIdentity, Embedding> embeddings, ImmutableStack<IIdentity> parentChain)
        {
            Parent = previous;
            IdentityProvider = identityProvider;
            Embeddings = embeddings ?? ImmutableDictionary<IIdentity, Embedding>.Empty;
            ParentChain = parentChain ?? ImmutableStack<IIdentity>.Empty;
        }
        /// <summary>
        /// Should create a new mutated context.
        /// </summary>
        /// <param name="embeddings">An optional dictionary of embeddings.</param>
        /// <param name="parentChain">An optional stack of parent identities.</param>
        /// <returns>A new context object.</returns>
        protected abstract C New(ImmutableDictionary<IIdentity, Embedding> embeddings = null, ImmutableStack<IIdentity> parentChain = null);

        /// <summary>
        /// Adds a Rest Value to the lexical scope of the (de-)serialization process.
        /// </summary>
        /// <param name="value">The Rest Value to add.</param>
        /// <returns>A new SerializationContext with added information from the Rest Value.</returns>
        public C With(IRestValue value)
        {
            var embeddings = Embeddings.AddRange(value.Embeddings
                .Select(e => (embedding: e, hasId: e.Object as IHasIdentity))
                .Where(e => e.hasId != null)
                .Select(e => new KeyValuePair<IIdentity, Embedding>(IdentityProvider.Translate(e.hasId.Id), e.embedding)));
            return New(embeddings);
        }
        /// <summary>
        /// Adds a Rest Value to the lexical scope of the (de-)serialization process.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="value">The Rest Value to add.</param>
        /// <returns>A new SerializationContext with added information from the Rest Value.</returns>
        public C With<T>(IRestValue<T> value)
        {
            var embeddings = Embeddings.AddRange(value.Embeddings
                .Select(e => (embedding: e, hasId: e.Object as IHasIdentity))
                .Where(e => e.hasId != null)
                .Select(e => new KeyValuePair<IIdentity, Embedding>(IdentityProvider.Translate(e.hasId.Id), e.embedding)));
            return New(embeddings);
        }
        /// <summary>
        /// Removes an object with specified identity value from the embeddings of the context.
        /// </summary>
        /// <param name="id">The identity value to remove.</param>
        /// <returns>A new SerializationContext without the specified object.</returns>
        public C Without(IIdentity id)
            => New(embeddings: Embeddings.Remove(IdentityProvider.Translate(id)));
        /// <summary>
        /// Contains the Parent context for this SerializationContext.
        /// </summary>
        public C Parent { get; }

        /// <summary>
        /// Tries to get an embedded object from the context.
        /// </summary>
        /// <param name="id">The identity value for the object.</param>
        /// <param name="result">An out parameter a found object will be assigned to.</param>
        /// <returns>True if an object with the specified id could be found, false otherwise.</returns>
        public bool TryGetEmbedding(IIdentity id, out Embedding result)
            => Embeddings.TryGetValue(IdentityProvider.Translate(id), out result);

        /// <summary>
        /// Checks if serialization is currently (deep) serializing the contents of an object with a specified identity value.
        /// </summary>
        /// <param name="id">The identity value to check.</param>
        /// <returns>True if serialization is currently (deep) serializing the contents of an object with the specified identity value.</returns>
        public bool IsInParentChain(IIdentity id)
            => ParentChain.Contains(id);
        /// <summary>
        /// Adds an identity value to the 'parent-chain'.
        /// </summary>
        /// <param name="id">The identity value to add.</param>
        /// <returns>A new SerializationContext with the specified identity value added to the parent chain.</returns>
        public C WithParent(IIdentity id)
            => New(parentChain: ParentChain.Push(id));
    }
    public class SerializationContext : SerializationContext<SerializationContext>
    {
        /// <summary>
        /// Creates a new and empty SerializationContext.
        /// </summary>
        /// <param name="identityProvider">The Rest identity provider to use for resolving and creating IIdentities.</param>
        /// <returns>A new and empty SerializationContext.</returns>
        public static SerializationContext Create(IIdentityProvider identityProvider) => new SerializationContext(identityProvider, null, null, null);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identityProvider">An identity provider.</param>
        /// <param name="previous">The context that calls this constructor should pass itself.</param>
        /// <param name="embeddings">An immutable dictionary of embeddings.</param>
        /// <param name="parentChain">A stack of parent identities.</param>
        public SerializationContext(IIdentityProvider identityProvider, SerializationContext previous, ImmutableDictionary<IIdentity, Embedding> embeddings, ImmutableStack<IIdentity> parentChain)
            : base(identityProvider, previous, embeddings, parentChain)
        { }
        /// <summary>
        /// Creates a new mutated context.
        /// </summary>
        /// <param name="embeddings">An optional dictionary of embeddings.</param>
        /// <param name="parentChain">An optional stack of parent identities.</param>
        /// <returns>A new context object.</returns>
        protected override SerializationContext New(ImmutableDictionary<IIdentity, Embedding> embeddings = null, ImmutableStack<IIdentity> parentChain = null)
            => new SerializationContext(IdentityProvider, this, embeddings ?? Embeddings, parentChain ?? ParentChain);
    }
}
