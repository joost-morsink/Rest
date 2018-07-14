using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    /// <summary>
    /// This class contains scoped data needed for Hal serialization and deserialization.
    /// </summary>
    public class HalContext
    {
        private readonly IRestIdentityProvider identityProvider;
        private readonly ImmutableDictionary<IIdentity, object> embeddings;

        /// <summary>
        /// Creates a new and empty HalContext.
        /// </summary>
        /// <param name="identityProvider">The Rest identity provider to use for resolving and creating IIdentities.</param>
        /// <returns>A new and empty HalContext.</returns>
        public static HalContext Create(IRestIdentityProvider identityProvider) => new HalContext(identityProvider, null);

        private HalContext(IRestIdentityProvider identityProvider, HalContext previous, ImmutableDictionary<IIdentity,object> embeddings = null)
        {
            this.Parent = previous;
            this.identityProvider = identityProvider;
            this.embeddings = embeddings ?? ImmutableDictionary<IIdentity,object>.Empty;
        }
        /// <summary>
        /// Adds a Rest Value to the lexical scope of the Hal (de-)serialization process.
        /// </summary>
        /// <param name="value">The Rest Value to add.</param>
        /// <returns>A new HalContext with added information from the Rest Value.</returns>
        public HalContext With(IRestValue value)
        {
            var e = embeddings.AddRange(value.Embeddings.OfType<IHasIdentity>().Select(o => new KeyValuePair<IIdentity, object>(identityProvider.Translate(o.Id), o)));
            return new HalContext(identityProvider, this, e);
        }
        /// <summary>
        /// Adds a Rest Value to the lexical scope of the Hal (de-)serialization process.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="value">The Rest Value to add.</param>
        /// <returns>A new HalContext with added information from the Rest Value.</returns>
        public HalContext With<T>(RestValue<T> value)
        {
            var e = embeddings.AddRange(value.Embeddings.OfType<IHasIdentity>().Select(o => new KeyValuePair<IIdentity, object>(identityProvider.Translate(o.Id), o)));
            return new HalContext(identityProvider, this, e);
        }
        /// <summary>
        /// Removes an object with specified identity value from the embeddings of the context.
        /// </summary>
        /// <param name="id">The identity value to remove.</param>
        /// <returns>A new HalContext without the specified object.</returns>
        public HalContext Without(IIdentity id)
            => new HalContext(identityProvider, this, embeddings.Remove(identityProvider.Translate(id)));
        /// <summary>
        /// Contains the Parent context for this HalContext.
        /// </summary>
        public HalContext Parent { get; }

        /// <summary>
        /// Tries to get an embedded object from the context.
        /// </summary>
        /// <param name="id">The identity value for the object.</param>
        /// <param name="result">An out parameter a found object will be assigned to.</param>
        /// <returns>True if an object with the specified id could be found, false otherwise.</returns>
        public bool TryGetEmbedding(IIdentity id, out object result)
            => embeddings.TryGetValue(identityProvider.Translate(id), out result);
        
    }
}