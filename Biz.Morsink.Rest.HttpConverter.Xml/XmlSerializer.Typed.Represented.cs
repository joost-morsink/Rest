using Biz.Morsink.Rest.Schema;
using System;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public partial class XmlSerializer
    {
        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed XmlSerializer for types that are represented through an ITypeRepresentation instance.
            /// </summary>
            public class Represented : Typed<T>
            {
                private readonly ITypeRepresentation representation;
                private readonly Type originalType;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                /// <param name="originalType">Equals typeof(T).</param>
                /// <param name="representation">The type representation instance to use for transformations.</param>
                public Represented(XmlSerializer parent, Type originalType, ITypeRepresentation representation) : base(parent)
                {
                    this.representation = representation;
                    this.originalType = originalType;
                }
                public override XElement Serialize(T item)
                {
                    var repr = representation.GetRepresentation(item);
                    var res = Parent.Serialize(repr);
                    return new XElement(StripName(originalType.Name), res.GetContent());
                }
                public override T Deserialize(XElement e)
                {
                    var repr = Parent.Deserialize(e, representation.GetRepresentationType(typeof(T)));
                    return (T)representation.GetRepresentable(repr,typeof(T));
                }
            }
        }

    }
}
