using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SerializationContext = Biz.Morsink.Rest.Serialization.SerializationContext;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    public class HtmlRestSerializer : Serializer<SerializationContext>
    {
        private readonly IRestIdentityProvider identityProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDescriptorCreator">A type descriptor creator.</param>
        /// <param name="identityProvider">A Rest identity provider.</param>
        /// <param name="converter">An optional data converter.</param>
        public HtmlRestSerializer(
            ITypeDescriptorCreator typeDescriptorCreator,
            IRestIdentityProvider identityProvider,
            IDataConverter converter = null)
            : base(new DecoratedTypeDescriptorCreator(typeDescriptorCreator)
                  .Decorate(tdc => new ITypeRepresentation[] {
                  }),
                 converter)
        {
            this.identityProvider = identityProvider;
        }
        protected override IForType CreateSerializer(Type ty)
        {
            if (typeof(IRestValue).IsAssignableFrom(ty))
                return CreateRestValueSerializer(ty);
            var ser = base.CreateSerializer(ty);
            if (typeof(IIdentity).IsAssignableFrom(ty))
                return CreateIdentitySerializer(ser, ty);
            else if (typeof(IHasIdentity).IsAssignableFrom(ty))
                return CreateHasIdentitySerializer(ser, ty);
            else
                return ser;
        }

        private IForType CreateHasIdentitySerializer(IForType ser, Type ty)
        {
            return (IForType)Activator.CreateInstance(typeof(HasIdentityType<>).MakeGenericType(ty), this, ser);
        }

        private IForType CreateIdentitySerializer(IForType ser, Type ty)
        {
            return (IForType)Activator.CreateInstance(typeof(IdentityType<>).MakeGenericType(ty), this, ser);
        }

        private IForType CreateRestValueSerializer(Type ty)
        {
            return (IForType)Activator.CreateInstance(typeof(RestValueType<>).MakeGenericType(ty), this);
        }
        private class IdentityType<T> : Typed<T>
            where T : IIdentity
        {
            private readonly Typed<T> inner;

            public new HtmlRestSerializer Parent => (HtmlRestSerializer)base.Parent;
            public IdentityType(HtmlRestSerializer parent, Typed<T> inner) : base(parent)
            {
                this.inner = inner;
            }

            public override T Deserialize(SerializationContext context, SItem item)
            {
                throw new NotSupportedException();
            }
            public override SItem Serialize(SerializationContext context, T item)
            {
                var res = (SObject)inner.Serialize(context, item);
                return new SObject(res.Properties.Append(new SProperty("Desc", new SValue($"{item.ForType.Name}({item.Value})"))));
            }
        }
        private class HasIdentityType<T> : Typed<T>
            where T : IHasIdentity
        {
            private readonly Typed<T> inner;
            public new HtmlRestSerializer Parent => (HtmlRestSerializer)base.Parent;
            public HasIdentityType(HtmlRestSerializer parent, Typed<T> inner) : base(parent)
            {
                this.inner = inner;
            }

            public override SItem Serialize(SerializationContext context, T item)
            {
                if (context.TryGetEmbedding(item.Id, out var embedding))
                    return Parent.Serialize(context, item.Id);
                else
                    return inner.Serialize(context, item);
            }

            public override T Deserialize(SerializationContext context, SItem item)
            {
                throw new NotSupportedException();
            }
        }
        private class RestValueType<T> : Typed<T>
            where T : IRestValue
        {
            public new HtmlRestSerializer Parent => (HtmlRestSerializer)base.Parent;
            public RestValueType(HtmlRestSerializer parent) : base(parent)
            {
            }
            public override T Deserialize(SerializationContext context, SItem item)
            {
                throw new NotSupportedException();
            }

            public override SItem Serialize(SerializationContext context, T item)
            {
                var val = Parent.Serialize(context, item.Value);
                var links = Parent.Serialize(context, item.Links);
                var embeddings = Parent.Serialize(context, item.Embeddings);

                return new SObject(
                    new SProperty("Value", val),
                    new SProperty("Links", links),
                    new SProperty("Embeddings", embeddings));
            }
        }
        public SItem Serialize(object o)
            => Serialize(SerializationContext.Create(identityProvider), o);
        public string ToHtmlPage(SItem val)
        {
            var html = ToHtml(val);
            return new XElement("html",
                new XElement("head",
                new XElement("style", @"table {
    border-collapse: collapse; 
}
td, th {
    border: solid 1px black;
    margin: 0px;
    padding: 5px;
}")),
                new XElement("body", html.Elements()))
                .ToString();

        }
        public XElement ToHtml(SItem val)
        {
            switch (val)
            {
                case SObject o:
                    return ToHtml(o);
                case SValue v:
                    return ToHtml(v);
                case SArray a:
                    return ToHtml(a);
                default:
                    return new XElement("b", "Error");
            }
        }
        public XElement ToHtml(SObject obj)
        {
            if (obj.Properties.Count == 0)
                return new XElement("td", " ");
            else
            {
                var propDict = obj.ToDictionary();
                if (propDict.Count == 2 && new[] { "Href", "Desc" }.All(propDict.ContainsKey))
                    return new XElement("td",
                        new XElement("a", new XAttribute("href", ((SValue)propDict["Href"]).Value),
                            ((SValue)propDict["Desc"]).Value));
                else
                    return new XElement("td",
                        new XElement("table",
                            obj.Properties.Select(p => new XElement("tr", new XElement("td", p.Name), ToHtml(p.Token)))));
            }
        }
        public XElement ToHtml(SObject obj, string[] props)
        {
            return new XElement("tr",
                props.GroupJoin(obj.Properties, p => p, p => p.Name, (p, sps) => sps.Any() ? ToHtml(sps.Select(sp => sp.Token).First()) : new XElement("td")));
        }
        public XElement ToHtml(SValue val)
        {
            return new XElement("td", val.Value);
        }
        public XElement ToHtml(SArray arr)
        {
            var props = arr.Content.OfType<SObject>().SelectMany(o => o.Properties.Select(p => p.Name)).Distinct().ToArray();
            if (props.Length > 0)
                return new XElement("td",
                    new XElement("table",
                        new XElement("tr", props.Select(p => new XElement("th", p))),
                        arr.Content.OfType<SObject>().Select(o => ToHtml(o, props))));
            else
                return new XElement("td",
                    new XElement("table",
                        arr.Content.OfType<SValue>().Select(v => new XElement("tr", new XElement("td", v.Value)))));
        }
    }
}
