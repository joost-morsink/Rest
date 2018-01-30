using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public class XmlSchemaTypeDescriptorVisitor : TypeDescriptorVisitor<XElement>
    {
        private static XNamespace XSD = "http://www.w3.org/2001/XMLSchema";
        private static XNamespace XSI = "http://www.w3.org/2001/XMLSchema-instance";
        private const string schema = nameof(schema);
        private const string complexType = nameof(complexType);
        private const string sequence = nameof(sequence);
        private const string element = nameof(element);
        private const string type = nameof(type);
        private const string name = nameof(name);
        private const string minOccurs = nameof(minOccurs);
        private const string maxOccurs = nameof(maxOccurs);
        private const string nillable = nameof(nillable);
        private const string boolean = xs + ":" + nameof(boolean);
        private const string @string = xs + ":" + nameof(@string);
        private const string integer = xs + ":" + nameof(integer);
        private const string dateTime = xs + ":" + nameof(dateTime);
        private const string @decimal = xs + ":" + nameof(@decimal);
        private const string any = xs + ":" + nameof(any);
        private const string xs = nameof(xs);
        private const string xsi = nameof(xsi);

        private Dictionary<string, XElement> types;
        public XmlSchemaTypeDescriptorVisitor()
        {
            types = new Dictionary<string, XElement>();
        }
        private string GetName(string name)
        {
            return name.Replace('+', '.');
        }
        public new XElement Visit(TypeDescriptor t)
        {
            var res = base.Visit(t);
            return new XElement(XSD + schema,
                new XAttribute(XNamespace.Xmlns + xs, XSD.NamespaceName),
                new XAttribute(XNamespace.Xmlns + xsi, XSI.NamespaceName),
                new XElement(XSD + element, new XAttribute(name, GetName(t.Name.Substring(t.Name.LastIndexOf('.') + 1))),
                new XAttribute(type, GetName(t.Name))),
                types.Values);
        }

        protected override XElement VisitArray(TypeDescriptor.Array a, XElement inner)
        {

            throw new NotImplementedException();
        }

        protected override XElement VisitBoolean(TypeDescriptor.Primitive.Boolean b)
            => new XElement("_", new XAttribute(type, boolean));


        protected override XElement VisitDateTime(TypeDescriptor.Primitive.DateTime dt)
            => new XElement("_", new XAttribute(type, dateTime));

        protected override XElement VisitFloat(TypeDescriptor.Primitive.Numeric.Float f)
            => new XElement("_", new XAttribute(type, @decimal));

        protected override XElement VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i)
            => new XElement("_", new XAttribute(type, integer));

        protected override XElement VisitIntersection(TypeDescriptor.Intersection i, XElement[] parts)
        {
            throw new NotImplementedException();
        }

        protected override XElement VisitNull(TypeDescriptor.Null n)
        {
            return null;
        }

        protected override XElement VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<XElement>[] props)
        {
            if (!types.ContainsKey(r.Name))
            {
                var schema = new XElement(XSD + complexType,
                    new XAttribute(name, GetName(r.Name)),
                    new XElement(XSD + sequence,
                        props.Select(prop =>
                            new XElement(XSD + element,
                                new XAttribute(name, prop.Name),
                                new XAttribute(type, GetName(prop.Type.Attribute(type)?.Value ?? any)),
                                new XAttribute(minOccurs, prop.Required ? 1 : 0),
                                new XAttribute(maxOccurs, 1)))));
                types[r.Name] = schema;
            }
            return new XElement("_", new XAttribute(type, GetName(r.Name)));
        }

        protected override XElement VisitReferable(TypeDescriptor.Referable r, XElement expandedDescriptor)
        {
            return expandedDescriptor;
        }

        protected override XElement VisitReference(TypeDescriptor.Reference r)
            => new XElement("_", new XAttribute(type, GetName(r.RefName)));

        protected override XElement VisitString(TypeDescriptor.Primitive.String s)
            => new XElement("_", new XAttribute(type, @string));

        protected override XElement VisitUnion(TypeDescriptor.Union u, XElement[] options)
        {
            if (options.Length == 2 && (options[0] == null || options[1] == null))
            {
                var basetype = options[0] ?? options[1];
                basetype.SetAttributeValue(nillable, true);
                return basetype;
            }
            return options.Length == 0 ? null : options[0];
        }

        protected override XElement VisitValue(TypeDescriptor.Value v, XElement inner)
        {
            throw new NotImplementedException();
        }
    }
}
