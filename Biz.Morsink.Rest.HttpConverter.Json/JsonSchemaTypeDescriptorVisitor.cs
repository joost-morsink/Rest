using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// A visitor to create a Json Schema for a TypeDescriptor.
    /// </summary>
    public class JsonSchemaTypeDescriptorVisitor : TypeDescriptorVisitor<JObject>
    {
        private Dictionary<string, string> done;
        private Dictionary<string, string> todo;
        private readonly TypeDescriptorCreator typeDescriptorCreator;
        private readonly IEnumerable<IJsonSchemaTranslator> translators;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDescriptorCreator">A TypeDescriptorCreator to resolve 'references'.</param>
        public JsonSchemaTypeDescriptorVisitor(TypeDescriptorCreator typeDescriptorCreator, IEnumerable<IJsonSchemaTranslator> translators)
        {
            this.typeDescriptorCreator = typeDescriptorCreator;
            this.translators = translators;
        }
        private string CamelCase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;
            if (char.IsUpper(name[0]))
                return char.ToLower(name[0]) + name.Substring(1);
            else
                return name;
        }
        /// <summary>
        /// This method transforms a TypeDescriptor to a JObject containing the Json schema.
        /// </summary>
        /// <param name="descriptor">A TypeDescriptor.</param>
        /// <returns>The Json schema for the TypeDescriptor.</returns>
        public JObject Transform(TypeDescriptor descriptor)
        {
            todo = new Dictionary<string, string>();
            done = new Dictionary<string, string>();
            done.Add(descriptor.Name, "#");
            var res = Visit(descriptor);
            if (todo.Count > 0)
            {
                res = AddDefinitions(res);
            }
            res["$schema"] = JsonSchema.JSON_SCHEMA_VERSION;
            return res;
        }
        public override JObject Visit(TypeDescriptor t)
        {
            var assoc = t.GetAssociatedType();
            if (assoc != null) { 
                foreach(var translator in translators)
                {
                    var res = translator.GetSchema(assoc);
                    if (res != null)
                        return res.Schema;
                }
            }
            return base.Visit(t);
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
        private JObject AddDefinitions(JObject result)
        {
            var defs = new JObject();
            while (todo.Count > 0)
            {
                var item = todo.First();
                todo.Remove(item.Key);
                done.Add(item.Key, item.Value);
                defs.Add(new JProperty(item.Value, Visit(typeDescriptorCreator.GetDescriptorByName(item.Key))));
            }
            result.Add(new JProperty("definitions", defs));
            return result;
        }
        protected override JObject VisitAny(TypeDescriptor.Any a)
        {
            return new JObject();
        }
        protected override JObject VisitArray(TypeDescriptor.Array a, JObject inner)
        {
            return new JObject(new JProperty("type", "array"), new JProperty("items", inner));
        }

        protected override JObject VisitBoolean(TypeDescriptor.Primitive.Boolean b)
        {
            return new JObject(new JProperty("type", "boolean"));
        }

        protected override JObject VisitDateTime(TypeDescriptor.Primitive.DateTime dt)
        {
            return new JObject(new JProperty("type", "string"), new JProperty("format", "date-time"));
        }

        protected override JObject VisitFloat(TypeDescriptor.Primitive.Numeric.Float f)
        {
            return new JObject(new JProperty("type", "number"));
        }

        protected override JObject VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i)
        {
            return new JObject(new JProperty("type", "number"));
        }

        protected override JObject VisitIntersection(TypeDescriptor.Intersection i, JObject[] parts)
        {
            return new JObject(new JProperty("allOf", new JArray(parts)));
        }

        protected override JObject VisitNull(TypeDescriptor.Null n)
        {
            return new JObject(new JProperty("type", "null"));
        }

        protected override JObject VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<JObject>[] props)
        {
            return new JObject(
                new JProperty("type", "object"),
                new JProperty("properties", new JObject(props.Select(x => new JProperty(CamelCase(x.Name), x.Type)))),
                new JProperty("required", new JArray(props.Where(p => p.Required).Select(p => CamelCase(p.Name)))));
        }
        protected override JObject VisitDictionary(TypeDescriptor.Dictionary d, JObject valueType)
        {
            return new JObject(
                new JProperty("type", "object"),
                new JProperty("additionalProperties", valueType));
        }
        protected override JObject VisitReference(TypeDescriptor.Reference r)
        {
            var shortName = GetSchemaAddress(r.RefName);

            return new JObject(new JProperty("$ref", shortName));
        }

        protected override JObject VisitReferable(TypeDescriptor.Referable r, JObject expandedDescriptor)
            => expandedDescriptor;

        protected override JObject VisitString(TypeDescriptor.Primitive.String s)
        {
            return new JObject(new JProperty("type", "string"));
        }

        protected override JObject VisitUnion(TypeDescriptor.Union u, JObject[] options)
        {
            return new JObject(new JProperty("anyOf", new JArray(options)));
        }

        protected override JObject VisitValue(TypeDescriptor.Value v, JObject inner)
        {
            return new JObject(new JProperty("enum", new JArray(v.InnerValue)));
        }
    }
}
