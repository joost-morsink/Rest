using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public class HalContext
    {
        private readonly HalContext previous;
        private readonly IRestIdentityProvider identityProvider;
        private readonly ImmutableDictionary<IIdentity, object> embeddings;

        public static HalContext Create(IRestIdentityProvider identityProvider) => new HalContext(identityProvider, null);

        private HalContext(IRestIdentityProvider identityProvider, HalContext previous, ImmutableDictionary<IIdentity,object> embeddings = null)
        {
            this.previous = previous;
            this.identityProvider = identityProvider;
            this.embeddings = embeddings ?? ImmutableDictionary<IIdentity,object>.Empty;
        }
        public HalContext With(IRestValue value)
        {
            var e = embeddings.AddRange(value.Embeddings.OfType<IHasIdentity>().Select(o => new KeyValuePair<IIdentity, object>(identityProvider.Translate(o.Id), o)));
            return new HalContext(identityProvider, this, e);
        }
        public HalContext Pop() => previous;

        public bool TryGetEmbedding(IIdentity id, out object result)
            => embeddings.TryGetValue(id, out result);
        
    }
}