﻿using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using static Biz.Morsink.Rest.HttpConverter.Xml.XsdConstants;
namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// An IXmlSchemaTranslator implementation for TypeDescriptors.
    /// </summary>
    public class XmlSchemaXmlSchemaTranslator : IXmlSchemaTranslator<TypeDescriptor>
    {
        private XmlSerializer serializer;
        private readonly Lazy<IEnumerable<IXmlSchemaTranslator>> translators;
        private readonly TypeDescriptorCreator typeDescriptorCreator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">An IServiceProvider instance.</param>
        public XmlSchemaXmlSchemaTranslator(IServiceProvider serviceProvider)
        {
            this.translators = new Lazy<IEnumerable<IXmlSchemaTranslator>>(() => serviceProvider.GetService<IEnumerable<IXmlSchemaTranslator>>());
            this.typeDescriptorCreator = serviceProvider.GetService<TypeDescriptorCreator>();
        }

        /// <summary>
        /// Gets typeof(TypeDescriptor).
        /// </summary>
        public Type ForType => typeof(TypeDescriptor);

        /// <summary>
        /// Gets a typed XmlSerializer instance for TypeDescriptor.
        /// </summary>
        /// <returns>A typed XmlSerializer instance for TypeDescriptor.</returns>
        public XmlSerializer.Typed<TypeDescriptor> GetConverter()
            => new XmlSerializer.Typed<TypeDescriptor>.Delegated(serializer, Serialize, Deserialize);

        /// <summary>
        /// Gets a schema for XSD.
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
            => new XmlSchema(
                new XElement(XSD + schema,
                    new XAttribute(XNamespace.Xmlns + xs, XSD.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + xsi, XSI.NamespaceName),
                    new XElement(XSD+include,
                        new XAttribute(schemaLocation, "https://www.w3.org/2009/XMLSchema/XMLSchema.xsd"))));

        /// <summary>
        /// Sets the parent XmlSerializer.
        /// This method is used internally.
        /// The instance is used to resolve serializers for dependencies.
        /// </summary>
        /// <param name="parent">The parent XmlSerializer.</param>
        public void SetSerializer(XmlSerializer parent)
        {
            serializer = parent;
        }

        XmlSerializer.IForType IXmlSchemaTranslator.GetConverter()
            => GetConverter();

        private XElement Serialize(TypeDescriptor item)
        {
            var specific = translators.Value.FirstOrDefault(t => typeDescriptorCreator.GetDescriptor(t.ForType)?.Equals(item) == true);
            if (specific == null)
            {
                var visitor = new XmlSchemaTypeDescriptorVisitor(typeDescriptorCreator);
                return visitor.Visit(item);
            }
            else
                return specific.GetSchema().Schema;
        }
        /// <summary>
        /// Returns null.
        /// Deserialization of schema's is not supported.
        /// </summary>
        /// <returns>null</returns>
        private TypeDescriptor Deserialize(XElement schema)
            => null;
    }
}
