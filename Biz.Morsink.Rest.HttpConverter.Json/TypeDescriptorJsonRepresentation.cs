using Biz.Morsink.Rest.HttpConverter.Json;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter
{
    /// <summary>
    /// A representation for the serialization of type descriptors to json.
    /// </summary>
    public class TypeDescriptorJsonRepresentation : SimpleTypeRepresentation<TypeDescriptor, SObject>
    {
        private readonly ITypeDescriptorCreator typeDescriptorCreator;
        private readonly IOptions<JsonHttpConverterOptions> jsonOptions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDescriptorCreator">A type descriptor creator, needed for reference serialization.</param>
        public TypeDescriptorJsonRepresentation(ITypeDescriptorCreator typeDescriptorCreator, IOptions<JsonHttpConverterOptions> jsonOptions)
        {
            this.typeDescriptorCreator = typeDescriptorCreator;
            this.jsonOptions = jsonOptions;
        }
        public override TypeDescriptor GetRepresentable(SObject representation)
        {
            throw new NotSupportedException();
        }

        public override SObject GetRepresentation(TypeDescriptor item)
        {
            if (typeof(TypeDescriptor).IsAssignableFrom(item.AssociatedType))
                return new SObject(new SProperty("$ref", new SValue(JsonSchema.JSON_SCHEMA_VERSION)));
            var visitor = new Visitor(typeDescriptorCreator,jsonOptions.Value.NamingStrategy);
            return visitor.Transform(item);
        }

        private class Visitor : TypeDescriptorVisitor<SObject>
        {
            private readonly ITypeDescriptorCreator typeDescriptorCreator;
            private readonly NamingStrategy naming;

            public Visitor(ITypeDescriptorCreator typeDescriptorCreator, NamingStrategy naming)
            {
                this.typeDescriptorCreator = typeDescriptorCreator;
                this.naming = naming;
            }
            private Dictionary<string, string> todo;
            private Dictionary<string, string> done;

            public SObject Transform(TypeDescriptor descriptor)
            {
                todo = new Dictionary<string, string>();
                done = new Dictionary<string, string>();
                done.Add(descriptor.Name, "#");
                var res = Visit(descriptor);
                if (todo.Count > 0)
                    res = AddDefinitions(res);

                return new SObject(res.Properties.Append(new SProperty("$schema", new SValue(JsonSchema.JSON_SCHEMA_VERSION))));
            }

            private string GetSchemaAddress(string name)
            {
                if (todo.TryGetValue(name, out var res))
                    return res;
                if (done.TryGetValue(name, out res))
                    return res;

                res = "#/definitions/" + (name?.Substring(name.LastIndexOf('.') + 1));
                todo.Add(name, res);

                return res;
            }
            private SObject AddDefinitions(SObject res)
            {
                var defs = new List<SProperty>();
                while (todo.Count > 0)
                {
                    var item = todo.First();
                    todo.Remove(item.Key);
                    done.Add(item.Key, item.Value);
                    defs.Add(new SProperty(item.Value, Visit(typeDescriptorCreator.GetDescriptorByName(item.Key))));
                }
                return new SObject(res.Properties.Append(new SProperty("definitions", new SObject(defs))));
            }

            protected override SObject VisitAny(TypeDescriptor.Any a)
                => new SObject();

            protected override SObject VisitArray(TypeDescriptor.Array a, SObject inner)
                => new SObject(
                    new SProperty("type", new SValue("array")),
                    new SProperty("items", inner));

            protected override SObject VisitBoolean(TypeDescriptor.Primitive.Boolean b)
                => new SObject(new SProperty("type", new SValue("boolean")));

            protected override SObject VisitDateTime(TypeDescriptor.Primitive.DateTime dt)
                => new SObject(
                    new SProperty("type", new SValue("string")),
                    new SProperty("format", new SValue("date-time")));

            protected override SObject VisitDictionary(TypeDescriptor.Dictionary d, SObject valueType)
                => new SObject(
                    new SProperty("type", new SValue("object")),
                    new SProperty("additionalProperties", valueType));

            protected override SObject VisitFloat(TypeDescriptor.Primitive.Numeric.Float f)
                => new SObject(new SProperty("type", new SValue("number")));


            protected override SObject VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i)
                => new SObject(new SProperty("type", new SValue("number")));

            protected override SObject VisitIntersection(TypeDescriptor.Intersection i, SObject[] parts)
                => new SObject(new SProperty("allOf", new SArray(parts)));

            protected override SObject VisitNull(TypeDescriptor.Null n)
                => new SObject(new SProperty("type", new SValue("null")));

            protected override SObject VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<SObject>[] props)
                => new SObject(
                    new SProperty("type", new SValue("object")),
                    new SProperty("properties",
                        new SObject(from p in props select new SProperty(p.Name, p.Type))),
                    new SProperty("required", new SArray(from p in props where p.Required select new SValue(naming.GetPropertyName(p.Name,false)))));

            protected override SObject VisitReferable(TypeDescriptor.Referable r, SObject expandedDescriptor)
                => expandedDescriptor;

            protected override SObject VisitReference(TypeDescriptor.Reference r)
            {
                var shortName = GetSchemaAddress(r.RefName);
                return new SObject(new SProperty("$ref", new SValue(shortName)));
            }

            protected override SObject VisitString(TypeDescriptor.Primitive.String s)
                => new SObject(new SProperty("type", new SValue("string")));


            protected override SObject VisitUnion(TypeDescriptor.Union u, SObject[] options)
                => new SObject(new SProperty("anyOf", new SArray(options)));

            protected override SObject VisitValue(TypeDescriptor.Value v, SObject inner)
                => new SObject(new SProperty("enum", new SValue(v.InnerValue)));
        }
    }
}
