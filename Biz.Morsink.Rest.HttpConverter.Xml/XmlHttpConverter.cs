using Biz.Morsink.Rest.AspNetCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Biz.Morsink.Rest.HttpConverter.Xml.XsdConstants;
namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public class XmlHttpConverter : AbstractHttpRestConverter
    {
        private readonly XmlSerializer serializer;

        public XmlHttpConverter(XmlSerializer serializer, IRestIdentityProvider identityProvider)
            : base(identityProvider)
        {
            this.serializer = serializer;
        }
        public override bool Applies(HttpContext context)
            => HasAcceptHeader(context.Request, "application/xml");

        public override object ParseBody(Type t, byte[] body)
        {
            using (var ms = new MemoryStream(body))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            using (var xr = XmlReader.Create(sr))
            {
                var element = XElement.Load(xr);
                return serializer.Deserialize(element, t);
            }
        }

        protected override void ApplyGeneralHeaders(HttpResponse httpResponse, RestResponse response)
        {
            httpResponse.ContentType = "application/xml";
        }

        protected override void ApplyHeaders(HttpResponse httpResponse, RestResponse response, IRestValue value)
        {
            UseLinkHeaders(httpResponse, value);
            UseSchemaLocationHeader(httpResponse, value);
        }

        protected override async Task WriteValue(Stream bodyStream, IRestValue value)
        {
            using (var ms = new MemoryStream())
            using (var wri = XmlWriter.Create(ms))
            {
                var element = serializer.Serialize(value.Value);
                element.SetAttributeValue(XNamespace.Xmlns + xsi, XSI.NamespaceName);
                element.WriteTo(wri);
                wri.Flush();
                ms.Seek(0L, SeekOrigin.Begin);
                await ms.CopyToAsync(bodyStream);
            }
        }
    }
}
