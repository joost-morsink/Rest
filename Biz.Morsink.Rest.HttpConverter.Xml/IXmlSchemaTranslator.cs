using Biz.Morsink.Rest.AspNetCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public interface IXmlSchemaTranslator : ISchemaTranslator<XmlSchema>
    {
        XmlSerializer.IForType GetConverter();
        void SetSerializer(XmlSerializer parent);
    }
    public interface IXmlSchemaTranslator<T> : ISchemaTranslator<T, XmlSchema>, IXmlSchemaTranslator
    {
        new XmlSerializer.Typed<T> GetConverter();
    }
}
