using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public class XmlSchemaTypeDescriptorVisitor : TypeDescriptorVisitor<XmlSchemaElement>
    {
        private XNamespace XSD = "http://www.w3.org/2001/XMLSchema";

        protected override XmlSchemaElement VisitArray(TypeDescriptor.Array a, XmlSchemaElement inner)
        {
            // Should take the XmlSchemaElement with maxoccurs = unbounded
            // What if ...?
            // Ref
            // complex type
            throw new NotImplementedException();
        }

        protected override XmlSchemaElement VisitBoolean(TypeDescriptor.Primitive.Boolean b)
            => new XmlSchemaElement.TypeRef("xs:boolean");


        protected override XmlSchemaElement VisitDateTime(TypeDescriptor.Primitive.DateTime dt)
            => new XmlSchemaElement.TypeRef("xs:dateTime");

        protected override XmlSchemaElement VisitFloat(TypeDescriptor.Primitive.Numeric.Float f)
            => new XmlSchemaElement.TypeRef("xs:decimal");

        protected override XmlSchemaElement VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i)
            => new XmlSchemaElement.TypeRef("xs:integer");

        protected override XmlSchemaElement VisitIntersection(TypeDescriptor.Intersection i, XmlSchemaElement[] parts)
        {
            throw new NotImplementedException();
        }

        protected override XmlSchemaElement VisitNull(TypeDescriptor.Null n)
        {
            throw new NotImplementedException();
        }

        protected override XmlSchemaElement VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<XmlSchemaElement>[] props)
        {
            var schema = new XElement(XSD + "element",
                new XAttribute("name", r.Name));
            return new XmlSchemaElement.Record(props.ToImmutableDictionary(p => p.Name, p => p.Type));
        }

        protected override XmlSchemaElement VisitReferable(TypeDescriptor.Referable r, XmlSchemaElement expandedDescriptor)
        {
            throw new NotImplementedException();
        }

        protected override XmlSchemaElement VisitReference(TypeDescriptor.Reference r)
        {
            throw new NotImplementedException();
        }

        protected override XmlSchemaElement VisitString(TypeDescriptor.Primitive.String s)
            => new XmlSchemaElement.TypeRef("xs:string");

        protected override XmlSchemaElement VisitUnion(TypeDescriptor.Union u, XmlSchemaElement[] options)
        {
            throw new NotImplementedException();
        }

        protected override XmlSchemaElement VisitValue(TypeDescriptor.Value v, XmlSchemaElement inner)
        {
            throw new NotImplementedException();
        }
    }
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
            public Record(IReadOnlyDictionary<string, XmlSchemaElement> properties)
            {
                Properties = properties.ToImmutableDictionary(x => x.Key, x => x.Value);
            }

            public IReadOnlyDictionary<string, XmlSchemaElement> Properties { get; }
        }
        public sealed class Concrete : XmlSchemaElement
        {
            public Concrete(XElement schema)
            {
                Schema = schema;
            }

            public XElement Schema { get; }
        }
    }
}
