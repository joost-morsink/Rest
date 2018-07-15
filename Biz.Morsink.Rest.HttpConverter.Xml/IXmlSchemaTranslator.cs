using Biz.Morsink.Rest.AspNetCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// This interface specifies types that convert objects to and from Xml as well as providing a schema(XSD) for those types.
    /// </summary>
    public interface IXmlSchemaTranslator : ISchemaTranslator<XmlSchema>
    {
        XmlSerializer.IForType GetConverter(Type type);
        void SetSerializer(XmlSerializer parent);
    }
    /// <summary>
    /// This interface specifies types that convert objects to and from Xml as well as providing a schema(XSD) for those types.
    /// </summary>
    /// <typeparam name="T">The type the translator applies to.</typeparam>
    public interface IXmlSchemaTranslator<T> : ISchemaTranslator<T, XmlSchema>, IXmlSchemaTranslator
    {
        new XmlSerializer.Typed<T> GetConverter(Type type);
    }
}
