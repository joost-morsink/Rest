using System;
using System.Collections.Generic;
using System.Text;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.OpenApi;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// OpenAPI Specification documents cannot be serialized in xml.
    /// </summary>
    public class OpenApiXmlSchemaTranslator : IXmlSchemaTranslator<Document>
    {
        /// <summary>
        /// Returns null.
        /// </summary>
        public XmlSerializer.Typed<Document> GetConverter(Type type)
            => null;

        /// <summary>
        /// Returns null.
        /// </summary>
        public XmlSchema GetSchema(Type type)
            => null;

        /// <summary>
        /// Empty implementation.
        /// No-operation.
        /// </summary>
        public void SetSerializer(XmlSerializer parent)
        { }

        XmlSerializer.IForType IXmlSchemaTranslator.GetConverter(Type type)
            => GetConverter(type);
    }
}
