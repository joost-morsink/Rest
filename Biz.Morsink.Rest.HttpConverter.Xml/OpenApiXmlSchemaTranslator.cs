using System;
using System.Collections.Generic;
using System.Text;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.OpenApi;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public class OpenApiXmlSchemaTranslator : IXmlSchemaTranslator<Document>
    {
        public Type ForType => typeof(Document);

        public XmlSerializer.Typed<Document> GetConverter()
            => throw new UnsupportedMediaTypeException();
        
        public XmlSchema GetSchema()
            => throw new UnsupportedMediaTypeException();

        public void SetSerializer(XmlSerializer parent)
        { }

        XmlSerializer.IForType IXmlSchemaTranslator.GetConverter()
            => GetConverter();
    }
}
