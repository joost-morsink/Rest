using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static Biz.Morsink.Rest.HttpConverter.Xml.XsdConstants;
namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public class XmlSchemaTypeDescriptorVisitor : TypeDescriptorVisitor<XElement>
    {
        private Dictionary<string, XElement> types;
        public XmlSchemaTypeDescriptorVisitor()
        {
            types = new Dictionary<string, XElement>();
        }
        private string GetName(string name)
        {
            return name.TrimStart('&', '+', '.').Replace('+', '.');
        }
        private string GetShortName(string name)
        {
            return GetName(name.Substring(name.LastIndexOf('.') + 1));
        }
        public new XElement Visit(TypeDescriptor t)
        {
            var res = base.Visit(t);
            return new XElement(XSD + schema,
                new XAttribute(XNamespace.Xmlns + xs, XSD.NamespaceName),
                new XAttribute(XNamespace.Xmlns + xsi, XSI.NamespaceName),
                new XElement(XSD + element, new XAttribute(name, GetShortName(t.Name)),
                new XAttribute(type, GetName(t.Name))),
                types.Values);
        }

        protected override XElement VisitArray(TypeDescriptor.Array a, XElement inner)
        {
            if (!types.ContainsKey(a.Name))
            {
                var schema = new XElement(XSD + complexType,
                    new XAttribute(name, "ArrayOf" + GetName(a.ElementType.Name)),
                        new XElement(XSD + sequence,
                            new XElement(XSD + element,
                                new XAttribute(name, GetShortName(a.ElementType.Name)),
                                new XAttribute(type, inner.Attribute(type)?.Value ?? (xs + @string)),
                                new XAttribute(minOccurs, 0),
                                new XAttribute(maxOccurs, "unbounded"))));
                types[a.Name] = schema;
            }
            return new XElement("_", new XAttribute(type, "ArrayOf" + GetName(a.ElementType.Name)));
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
                    new XElement(XSD + all,
                        props.Select(prop =>
                            new XElement(XSD + element,
                                new XAttribute(name, GetName(prop.Name)),
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

        private string valueTypeToString(TypeDescriptor td)
        {
            if (td is TypeDescriptor.Primitive)
            {
                if (td is TypeDescriptor.Primitive.String)
                    return @string;
                else if (td is TypeDescriptor.Primitive.Numeric.Integral)
                    return integer;
                else if (td is TypeDescriptor.Primitive.Numeric.Float)
                    return @decimal;
                else if (td is TypeDescriptor.Primitive.Boolean)
                    return boolean;
                else if (td is TypeDescriptor.Primitive.DateTime)
                    return dateTime;
                else
                    return @string;
            }
            else return any;
        }
        protected override XElement PrevisitUnion(TypeDescriptor.Union u)
        {
            if (u.Options.Count > 0 && u.Options.All(o => o is TypeDescriptor.Value))
            {
                var schema = new XElement(XSD + simpleType,
                    new XAttribute(name, u.Name),
                    new XElement(XSD + restriction,
                        new XAttribute(@base, valueTypeToString(((TypeDescriptor.Value)u.Options.First()).BaseType)),
                        u.Options.Cast<TypeDescriptor.Value>().Select(o => new XElement(XSD + enumeration, new XAttribute(value, o.InnerValue)))));
                types[u.Name] = schema;
                return new XElement("_", new XAttribute("type", u.Name));
            }
            else
                return base.PrevisitUnion(u);
        }
        protected override XElement VisitUnion(TypeDescriptor.Union u, XElement[] options)
        {
            if (options.Length == 2 && (options[0] == null || options[1] == null))
            {
                var basetype = options[0] ?? options[1];
                basetype.SetAttributeValue(nillable, true);
                return basetype;
            }
            else if (options.Length == 0)
                return null;
            else
            {
                if (!types.ContainsKey(u.Name))
                {
                    var schema = new XElement(XSD + complexType,
                        new XAttribute(name, GetName(u.Name)),
                        new XElement(XSD + choice,
                            options.Select(opt =>
                                new XElement(XSD + element,
                                    new XAttribute(name, opt.Name.LocalName),
                                    new XAttribute(type, opt.Attribute(type)?.Value ?? any),
                                    new XAttribute(minOccurs, opt.Attribute(minOccurs)?.Value ?? "1"),
                                    new XAttribute(maxOccurs, 1)))));
                    types[u.Name] = schema;
                }
                return new XElement("_", new XAttribute("type", u.Name));
            }
        }

        protected override XElement VisitValue(TypeDescriptor.Value v, XElement inner)
        {
            throw new NotImplementedException();
        }
    }
}
