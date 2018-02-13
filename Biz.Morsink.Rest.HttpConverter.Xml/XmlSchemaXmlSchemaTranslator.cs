using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public class XmlSchemaXmlSchemaTranslator : IXmlSchemaTranslator<TypeDescriptor>
    {
        private XmlSerializer serializer;

        public XmlSchemaXmlSchemaTranslator()
        {

        }

        public Type ForType => typeof(TypeDescriptor);

        public XmlSerializer.Typed<TypeDescriptor> GetConverter()
            => new XmlSerializer.Typed<TypeDescriptor>.Delegated(serializer, Serialize, Deserialize);

        public XmlSchema GetSchema()
            => null;

        public void SetSerializer(XmlSerializer parent)
        {
            serializer = parent;
        }

        XmlSerializer.IForType IXmlSchemaTranslator.GetConverter()
            => GetConverter();

        private XElement Serialize(TypeDescriptor item)
        {
            var visitor = new XmlSchemaTypeDescriptorVisitor();
            return visitor.Visit(item);
        }
        private TypeDescriptor Deserialize(XElement schema)
            => null;
    }
}
