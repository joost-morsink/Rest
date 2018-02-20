using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// This type represents Xml schemas.
    /// At the moment the schema can be passed to the constructor as an XElement.
    /// </summary>
    public class XmlSchema
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schema">The actual XSD schema.</param>
        public XmlSchema(XElement schema)
        {
            Schema = schema;
        }
        /// <summary>
        /// Gets the schema (XSD).
        /// </summary>
        public XElement Schema { get; }
    }
}
