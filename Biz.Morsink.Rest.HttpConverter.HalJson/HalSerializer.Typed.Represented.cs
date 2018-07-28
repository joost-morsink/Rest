using System;
using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json.Linq;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {

        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed HalSerializer for types that are represented through an ITypeRepresentation instance.
            /// </summary>
            public class Represented : Typed<T>
            {
                private readonly ITypeRepresentation representation;
                private readonly Type originalType;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent HalSerializer instance.</param>
                /// <param name="originalType">The original type that is passed for serialization.</param>
                /// <param name="representation">The type representation instance to use for serialization.</param>
                public Represented(HalSerializer parent, Type originalType, ITypeRepresentation representation) : base(parent)
                {
                    this.representation = representation;
                    this.originalType = originalType;
                }
                public override JToken Serialize(HalContext context, T item)
                {
                    var repr = representation.GetRepresentation(item);
                    var res = Parent.Serialize(context, repr);
                    return res;
                }
                public override T Deserialize(HalContext context, JToken token)
                {
                    var repr = Parent.Deserialize(representation.GetRepresentationType(typeof(T)), context, token);
                    return (T)representation.GetRepresentable(repr);
                }
            }
        }
    }
}
