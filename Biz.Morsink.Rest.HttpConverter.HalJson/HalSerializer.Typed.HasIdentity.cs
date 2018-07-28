using System;
using Biz.Morsink.Identity;
using Newtonsoft.Json.Linq;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {

        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed HalSerializer for types that implement IHasIdentity.
            /// </summary>
            public class HasIdentity : Typed<T>
            {
                private readonly Typed<T> fallback;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                /// <param name="fallback">A fallback serializer, to serialize the type as if it didn't implement IHasIdentity.</param>
                public HasIdentity(HalSerializer parent, Typed<T> fallback)
                    : base(parent)
                {
                    if (!typeof(IHasIdentity).IsAssignableFrom(typeof(T)))
                        throw new ArgumentException("Type does not implement IHasIdentity.");
                    this.fallback = fallback;
                }

                public override T Deserialize(HalContext context, JToken token)
                {
                    if (token is JObject obj)
                    {
                        if (obj["id"] != null)
                        {
                            var id = Parent.Deserialize<IIdentity>(context, obj["id"]);
                            if (id != null && context.TryGetEmbedding(id, out var res) && res is T t)
                                return t;
                        }
                    }

                    return fallback.Deserialize(context, token);
                }

                public override JToken Serialize(HalContext context, T item)
                {
                    var id = ((IHasIdentity)item).Id;
                    if (context.TryGetEmbedding(id, out _))
                        return new JObject(new JProperty("id", Parent.Serialize(context, id)));
                    else
                        return fallback.Serialize(context, item);
                }
            }
        }
    }
}
