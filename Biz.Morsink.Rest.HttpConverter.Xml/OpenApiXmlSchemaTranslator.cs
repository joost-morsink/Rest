using System;
using System.Collections.Generic;
using System.Text;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.OpenApi;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// OpenAPI Specification documents cannot be serialized in xml.
    /// This class is designed to cause UnsupportedMediaTypeExceptions to make the Rest Asp.Net core module return status 415.
    /// </summary>
    public class OpenApiXmlSchemaTranslator : IXmlSchemaTranslator<Document>
    {
        /// <summary>
        /// This translator applies to the OpenAPI Document class.
        /// </summary>
        public Type ForType => typeof(Document);

        /// <summary>
        /// Throws an UnsupportedMediaTypeException.
        /// </summary>
        public XmlSerializer.Typed<Document> GetConverter()
            => throw new UnsupportedMediaTypeException();

        /// <summary>
        /// Throws an UnsupportedMediaTypeException.
        /// </summary>
        public XmlSchema GetSchema()
            => throw new UnsupportedMediaTypeException();

        /// <summary>
        /// Empty implementation.
        /// No-operation.
        /// </summary>
        public void SetSerializer(XmlSerializer parent)
        { }

        XmlSerializer.IForType IXmlSchemaTranslator.GetConverter()
            => GetConverter();
    }
}
