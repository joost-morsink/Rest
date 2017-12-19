using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    //public class XmlSchemaTypeDescriptorVisitor : TypeDescriptorVisitor<XmlSchemaElement>
    //{
    //    private XNamespace XSD = "http://www.w3.org/2001/XMLSchema";


    //}
    public abstract class XmlSchemaElement
    {
        public sealed class TypeRef : XmlSchemaElement
        {
            public TypeRef(string refName)
            {
                RefName = refName;
            }

            public string RefName { get; }
        }
        public sealed class Record : XmlSchemaElement
        {
            public Record()
            {

            }
        }

    }
}
