using System;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public partial class XmlSerializer
    {
        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed XmlSerializer for which the operations are delegated to functions.
            /// </summary>
            public class Delegated : Typed<T>
            {
                private readonly Func<T, XElement> serializer;
                private readonly Func<XElement, T> deserializer;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">The parent serializer.</param>
                /// <param name="serializer">A function used for serialization.</param>
                /// <param name="deserializer">A function used for deserialization.</param>
                public Delegated(XmlSerializer parent, Func<T, XElement> serializer, Func<XElement, T> deserializer) : base(parent)
                {
                    this.serializer = serializer;
                    this.deserializer = deserializer;
                }

                public override T Deserialize(XElement e)
                    => deserializer(e);

                public override XElement Serialize(T item)
                    => serializer(item);
            }
        }

    }
}
