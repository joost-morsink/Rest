using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// This class generates a table for successful Rest values of a certain type.
    /// It is used by the DefaultHtmlGenerator and is not meant to be used for production scenarios.
    /// It uses reflection and might be slow.
    /// </summary>
    /// <typeparam name="T">The value type of the Rest value.</typeparam>
    public class TableGenerator<T> : AbstractSpecificHtmlGenerator<T>
    {
        static TableGenerator()
        {
            makeSingleMethod = typeof(TableGenerator<T>).GetMethod(nameof(MakeSingle), BindingFlags.NonPublic | BindingFlags.Instance);
            makeMultiMethod = typeof(TableGenerator<T>).GetMethod(nameof(MakeMulti), BindingFlags.NonPublic | BindingFlags.Instance);
        }
        private static readonly MethodInfo makeSingleMethod;
        private static readonly MethodInfo makeMultiMethod;
        private readonly IEnumerable<ITypeRepresentation> typeRepresentations;
        private readonly IdentityRepresentation identityRepresentation;

        public TableGenerator(IEnumerable<ITypeRepresentation> typeRepresentations)
        {
            this.typeRepresentations = typeRepresentations;
            identityRepresentation = typeRepresentations.OfType<IdentityRepresentation>().First();
        }

        public override string GenerateHtml(IRestValue<T> value)
        {

            return string.Format(
                @"<html><head>
<style>
table {{
    border-collapse: collapse; 
}}
td, th {{
    border: solid 1px black;
    margin: 0px;
    padding: 5px;
}}
</style>
</head>
<body>
<h3>Value</h3>
{0}
<h3>Links</h3>
{1}
<h3>Embeddings</h3>
{2}
</body>
</html>", Generate(value.Value).ToString(), MakeMulti(value.Links).Element("table"), MakeMulti(value.Embeddings).Element("table"));
        }

        public XElement Generate(T value)
        {
            foreach (var typeRep in typeRepresentations)
                if (typeRep.IsRepresentable(typeof(T)))
                    return Value(typeRep.GetRepresentation(value));
            return MakeSingle(value).Element("table");
        }

        private XElement MakeSingle<U>(U value)
        {
            var properties = typeof(U).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            return new XElement("td",
                new XElement("table",
                    properties.Select(p =>
                        new XElement("tr",
                            new XElement("th",
                                p.Name),
                            Value(p.GetValue(value))))));
        }
        private bool IsPrimitiveType(Type t)
            => t.IsPrimitive || t == typeof(string) || t == typeof(DateTime);

        private XElement MakeMulti<U>(IEnumerable<U> values)
        {
            if (IsPrimitiveType(typeof(U)) ||
                    typeRepresentations.Select(tr => tr.GetRepresentationType(typeof(U)))
                    .Where(rep => rep != null)
                    .Select(rep => IsPrimitiveType(rep))
                    .FirstOrDefault())
            {
                return new XElement("td",
                    new XElement("table",
                        values.Select(value =>
                        new XElement("tr", Value(value)))));
            }
            else
            {
                var properties = typeof(U).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                return new XElement("td",
                    new XElement("table",
                        new XElement("tr",
                            properties.Select(p => new XElement("th",
                                p.Name))),
                        values.Select(value =>
                            new XElement("tr",
                                properties.Select(p => Value(p.GetValue(value)))))));
            }
        }
        public XElement Value(object value)
        {
            if (value == null)
                return new XElement("td");
            var type = value.GetType();
            if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime))
                return new XElement("td",
                    value);
            if (value is Type t)
                return new XElement("td", $"{t.Namespace}.{t.Name}");

            if (value is IIdentity id)
                return new XElement("td",
                    new XElement("a",
                        new XAttribute("href", identityRepresentation.GetRepresentation(id).Href),
                        $"{id.ForType.Name}({IdValueString(id.Value)})"));

            foreach (var typeRep in typeRepresentations)
                if (typeRep.IsRepresentable(type))
                    return Value(typeRep.GetRepresentation(value));

            var coll = GetElementType(type);
            if (coll != null)
                return (XElement)makeMultiMethod.MakeGenericMethod(coll).Invoke(this, new[] { value });


            return (XElement)makeSingleMethod.MakeGenericMethod(type).Invoke(this, new[] { value });
        }

        private string IdValueString(object value)
        {
            if (value == null)
                return null;
            else if (value is IDictionary<string, string> dict)
                return string.Join(", ", dict.Select(kvp => $"{kvp.Key} -> {kvp.Value}"));
            else
                return value?.ToString();
        }

        private static Type GetElementType(Type type)
            => type.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GetGenericArguments().Length == 1 && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();

    }
}
