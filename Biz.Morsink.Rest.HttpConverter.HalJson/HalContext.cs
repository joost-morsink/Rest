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
    public class HalContext : SerializationContext
    {
        /// <summary>
        /// Creates a new and empty HalContext.
        /// </summary>
        /// <param name="identityProvider">The Rest identity provider to use for resolving and creating IIdentities.</param>
        /// <returns>A new and empty HalContext.</returns>
        public static new HalContext Create(IRestIdentityProvider identityProvider) => new HalContext(identityProvider, null);

        private HalContext(IRestIdentityProvider identityProvider, HalContext previous, ImmutableDictionary<IIdentity, object> embeddings = null, ImmutableStack<IIdentity> parentChain = null)
            : base(identityProvider, previous, embeddings, parentChain)
        {
        }
        protected override SerializationContext New(ImmutableDictionary<IIdentity, object> embeddings = null, ImmutableStack<IIdentity> parentChain = null)
            => new HalContext(IdentityProvider, this, embeddings ?? Embeddings, parentChain ?? ParentChain);
        /// <summary>
        /// Adds a Rest Value to the lexical scope of the Hal (de-)serialization process.
        /// </summary>
        /// <param name="value">The Rest Value to add.</param>
        /// <returns>A new HalContext with added information from the Rest Value.</returns>
        public new HalContext With(IRestValue value)
            => (HalContext)base.With(value);
        /// <summary>
        /// Adds a Rest Value to the lexical scope of the Hal (de-)serialization process.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="value">The Rest Value to add.</param>
        /// <returns>A new HalContext with added information from the Rest Value.</returns>
        public new HalContext With<T>(IRestValue<T> value)
            => (HalContext)base.With(value);
        /// <summary>
        /// Removes an object with specified identity value from the embeddings of the context.
        /// </summary>
        /// <param name="id">The identity value to remove.</param>
        /// <returns>A new HalContext without the specified object.</returns>
        public new HalContext Without(IIdentity id)
            => (HalContext)base.Without(id);
        /// <summary>
        /// Contains the Parent context for this HalContext.
        /// </summary>
        public new HalContext Parent => (HalContext)base.Parent;

    }
}