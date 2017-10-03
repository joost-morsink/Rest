using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    public class JsonSchemaTypeDescriptorVisitor : TypeDescriptorVisitor<JObject>
    {
        public const string DATETIME_REGEX = "^[0-9]{4}-[0-9]{2}-[0-9]{2}(T[0-9]{2}:[0-9]{2}(:[0-9]{2}(.[0-9]+)?)?Z)?$";
        public const string JSON_SCHEMA_VERSION = "http://json-schema.org/draft-04/schema#";
        public Dictionary<string, string> done;
        private Dictionary<string, string> todo;
        public JsonSchemaTypeDescriptorVisitor()
        {

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
            res["$schema"] = JSON_SCHEMA_VERSION;
            return res;
        }
        private string GetSchemaAddress(string name)
        {
            if (todo.TryGetValue(name, out var res))
                return res;
            if (done.TryGetValue(name, out res))
                return res;
            
            res = "#/definitions/"+( name?.Substring(name.LastIndexOf('.') + 1));
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
                defs.Add(new JProperty(item.Value, Visit(TypeDescriptorCreator.GetDescriptorByName(item.Key))));
            }
            result.Add(new JProperty("definitions", defs));
            return result;
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

        protected override JObject VisitIdentity(TypeDescriptor.Identity id, JObject inner)
        {
            return new JObject(
                new JProperty("properties", new JObject(
                    new JProperty("href", new JObject(
                        new JProperty("type", "string"))))));
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

        protected override JObject VisitReference(TypeDescriptor.Reference r)
        {
            var shortName = GetSchemaAddress(r.RefName);

            return new JObject(new JProperty("$ref", shortName));
        }

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
            return new JObject(new JProperty("enum", new JArray(v)));
        }
    }
}
