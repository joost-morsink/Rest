using Biz.Morsink.DataConvert;
using System.Xml.Linq;
using static Biz.Morsink.Rest.HttpConverter.Xml.XsdConstants;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public partial class XmlSerializer
    {
        public abstract partial class Typed<T>
        {
            /// <summary>
            /// Typed XmlSerializer for simple (primitive) 'tostring' types.
            /// </summary>
            public class Simple : Typed<T>
            {
                private readonly IDataConverter converter;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">A reference to the parent XmlSerializer instance.</param>
                /// <param name="converter">An IDataConverter instance for converting to and from string.</param>
                public Simple(XmlSerializer parent, IDataConverter converter) : base(parent)
                {
                    this.converter = converter;
                }

                public override T Deserialize(XElement e) => converter.Convert(e.Value).TryTo(out T res) ? res : default;
                public override XElement Serialize(T item)
                {
                    var ty = item.GetType();
                    if (converter.Convert(item).TryTo(out string str) && str != null)
                        return new XElement("simple", converter.Convert(item).To<string>());
                    else
                        return new XElement("simple", new XAttribute(XSI + nil, true));
                }

            }
        }

    }
}
