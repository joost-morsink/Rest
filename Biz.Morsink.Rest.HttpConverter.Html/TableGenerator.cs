using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    public class TableGenerator<T> : AbstractSpecificHtmlGenerator<T>
    {
        public TableGenerator()
        {
        }


        public override string GenerateHtml(RestValue<T> value)
        {
            return Generate(value.Value).ToString();
        }

        public XElement Generate(T value)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            return new XElement("table",
                properties.Select(p =>
                    new XElement("tr",
                        new XElement("th", p.Name),
                        Value(p.GetValue(value)))));
        }
        public XElement Value(object value)
        {
            if (value == null)
                return new XElement("td");
            var type = value.GetType();
            if (type.IsPrimitive || type == typeof(string))
                return new XElement("td", value);

            if (typeof(IIdentity).IsAssignableFrom(type))
                return new XElement("td", ((IIdentity)value).Value);

            dynamic gen = Activator.CreateInstance(typeof(TableGenerator<>).MakeGenericType(type));
            return gen.Generate(value);
        }
    }
}
