using Biz.Morsink.Identity;
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
            makeSingleMethod = typeof(TableGenerator<T>).GetMethod(nameof(MakeSingle), BindingFlags.NonPublic | BindingFlags.Static);
            makeMultiMethod = typeof(TableGenerator<T>).GetMethod(nameof(MakeMulti), BindingFlags.NonPublic | BindingFlags.Static);
        }
        private static MethodInfo makeSingleMethod;
        private static MethodInfo makeMultiMethod;


        public override string GenerateHtml(RestValue<T> value)
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
<body>
{0}
</body>
</html", Generate(value.Value).ToString());
        }

        public XElement Generate(T value)
        {
            return MakeSingle(value).Element("table");
        }

        private static XElement MakeSingle<U>(U value)
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
        private static XElement MakeMulti<U>(IEnumerable<U> values)
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
        public static XElement Value(object value)
        {
            if (value == null)
                return new XElement("td");
            var type = value.GetType();
            if (type.IsPrimitive || type == typeof(string))
                return new XElement("td",
                    value);

            var coll = GetElementType(type);
            if (coll != null)
                return (XElement)makeMultiMethod.MakeGenericMethod(coll).Invoke(null, new[] { value });

            if (typeof(IIdentity).IsAssignableFrom(type))
                return new XElement("td",
                    ((IIdentity)value).Value);

            return (XElement)makeSingleMethod.MakeGenericMethod(type).Invoke(null, new[] { value });
        }
        private static Type GetElementType(Type type)
            => type.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GetGenericArguments().Length == 1 && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();

    }
}
