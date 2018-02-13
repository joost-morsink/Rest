using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public class XmlSchema
    {
        public XmlSchema(XElement schema)
        {
            Schema = schema;
        }

        public XElement Schema { get; }
    }
}
