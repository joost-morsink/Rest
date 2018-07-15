﻿using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// This class provides a contextual object for use in serialization.
    /// </summary>
    public class SerializationContext
    {
        protected IRestIdentityProvider IdentityProvider { get; }
        protected ImmutableDictionary<IIdentity, object> Embeddings { get; }

        /// <summary>
        /// Creates a new and empty SerializationContext.
        /// </summary>
        /// <param name="identityProvider">The Rest identity provider to use for resolving and creating IIdentities.</param>
        /// <returns>A new and empty SerializationContext.</returns>
        public static SerializationContext Create(IRestIdentityProvider identityProvider) => new SerializationContext(identityProvider, null);

        protected SerializationContext(IRestIdentityProvider identityProvider, SerializationContext previous, ImmutableDictionary<IIdentity, object> embeddings = null)
        {
            Parent = previous;
            IdentityProvider = identityProvider;
            Embeddings = embeddings ?? ImmutableDictionary<IIdentity, object>.Empty;
        }
        protected virtual SerializationContext New(ImmutableDictionary<IIdentity, object> embeddings = null)
            => new SerializationContext(IdentityProvider, this, embeddings ?? Embeddings);

        /// <summary>
        /// Adds a Rest Value to the lexical scope of the Hal (de-)serialization process.
        /// </summary>
        /// <param name="value">The Rest Value to add.</param>
        /// <returns>A new SerializationContext with added information from the Rest Value.</returns>
        public SerializationContext With(IRestValue value)
        {
            var e = Embeddings.AddRange(value.Embeddings.OfType<IHasIdentity>().Select(o => new KeyValuePair<IIdentity, object>(IdentityProvider.Translate(o.Id), o)));
            return New(embeddings: e);
        }
        /// <summary>
        /// Adds a Rest Value to the lexical scope of the Hal (de-)serialization process.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="value">The Rest Value to add.</param>
        /// <returns>A new SerializationContext with added information from the Rest Value.</returns>
        public SerializationContext With<T>(RestValue<T> value)
        {
            var e = Embeddings.AddRange(value.Embeddings.OfType<IHasIdentity>().Select(o => new KeyValuePair<IIdentity, object>(IdentityProvider.Translate(o.Id), o)));
            return New(embeddings: e);
        }
        /// <summary>
        /// Removes an object with specified identity value from the embeddings of the context.
        /// </summary>
        /// <param name="id">The identity value to remove.</param>
        /// <returns>A new SerializationContext without the specified object.</returns>
        public SerializationContext Without(IIdentity id)
            => New(embeddings: Embeddings.Remove(IdentityProvider.Translate(id)));
        /// <summary>
        /// Contains the Parent context for this SerializationContext.
        /// </summary>
        public SerializationContext Parent { get; }

        /// <summary>
        /// Tries to get an embedded object from the context.
        /// </summary>
        /// <param name="id">The identity value for the object.</param>
        /// <param name="result">An out parameter a found object will be assigned to.</param>
        /// <returns>True if an object with the specified id could be found, false otherwise.</returns>
        public bool TryGetEmbedding(IIdentity id, out object result)
            => Embeddings.TryGetValue(IdentityProvider.Translate(id), out result);

    }
}
