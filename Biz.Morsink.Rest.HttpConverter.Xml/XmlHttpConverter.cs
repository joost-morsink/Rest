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
using Biz.Morsink.Rest.AspNetCore.Utils;
using Microsoft.Extensions.Options;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// Component that converts HTTP Xml bodies from and to Rest requests and responses.
    /// </summary>

    public class XmlHttpConverter : AbstractHttpRestConverter
    {
        private readonly XmlSerializer serializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serializer">An XmlSerializer instance to use for serialization and deserialization.</param>
        /// <param name="identityProvider">A Rest identity provider for mapping urls to and from identity values.</param>
        public XmlHttpConverter(XmlSerializer serializer, IRestIdentityProvider identityProvider, IOptions<RestAspNetCoreOptions> options)
            : base(identityProvider, options)
        {
            this.serializer = serializer;
        }
        /// <summary>
        /// Determines if the XML converter applies to the given HttpContext.
        /// This converter applies when the Accept header specifies application/xml.
        /// </summary>
        /// <param name="context">The HttpContext associated with the HTTP Request.</param>
        /// <returns>A score ranging from 0 to 1.</returns>
        public override decimal AppliesScore(HttpContext context)
            => ScoreAcceptHeader(context.Request, "application/xml");
        /// <summary>
        /// Parses XML bodies if the type is known.
        /// </summary>
        /// <param name="t">The expected type of the body.</param>
        /// <param name="body">A byte array containing the raw HTTP body.</param>
        /// <returns>A parsed object.</returns>
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
        /// <summary>
        /// Applies the Content-Type header value application/xml to the Http response.
        /// </summary>
        /// <param name="httpResponse">The Http response</param>
        /// <param name="response">The Rest response. (ignored)</param>
        protected override void ApplyGeneralHeaders(HttpResponse httpResponse, RestResponse response)
        {
            httpResponse.ContentType = "application/xml";
        }
        /// <summary>
        /// Applies Http headers to the Http response. 
        /// The XmlHttpConverter applies both Link headers and the Schema-Location header.
        /// </summary>
        /// <param name="httpResponse">The Http response.</param>
        /// <param name="response">The Rest response.</param>
        /// <param name="value">The Rest value to be serialized to the body.</param>
        /// <param name="prefixes">The Rest prefix container for this response.</param>
        protected override void ApplyHeaders(HttpResponse httpResponse, RestResponse response, IRestValue value, RestPrefixContainer prefixes)
        {
            UseLinkHeaders(httpResponse, value);
            UseSchemaLocationHeader(httpResponse, value);
        }
        /// <summary>
        /// Writes the Rest value to the a (Http response body) stream.
        /// </summary>
        /// <param name="bodyStream">The stream to write the value to.</param>
        /// <param name="value">The value to be written</param>
        /// <returns>An asynchronous result. (Task)</returns>
        protected override async Task WriteValue(Stream bodyStream, RestResponse response, IRestResult result, IRestValue value)
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
