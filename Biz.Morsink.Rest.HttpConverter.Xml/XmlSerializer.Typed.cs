using System;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public partial class XmlSerializer
    {
        /// <summary>
        /// Abstract base class for serializers that handle a specific single type.
        /// </summary>
        /// <typeparam name="T">The type the serializer handles.</typeparam>
        public abstract partial class Typed<T> : IForType
        {
            /// <summary>
            /// Gets a reference to the parent XmlSerializer instance.
            /// </summary>
            protected XmlSerializer Parent { get; }
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
            public Typed(XmlSerializer parent)
            {
                Parent = parent;
            }
            /// <summary>
            /// Should implement serialization for objects of type T.
            /// </summary>
            /// <param name="item">The object to serialize.</param>
            /// <returns>The serialization of the object as an XElement.</returns>
            public abstract XElement Serialize(T item);
            /// <summary>
            /// Should implement deserialization to objects of type T.
            /// </summary>
            /// <param name="e">The XElement to deserialize.</param>
            /// <returns>A deserialized object of type T.</returns>
            public abstract T Deserialize(XElement e);

            Type IForType.ForType => typeof(T);
            XElement IForType.Serialize(object item) => Serialize((T)item);
            object IForType.Deserialize(XElement element) => Deserialize(element);
        }

    }
}
