using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    /// <summary>
    /// Options class for the Xml HTTP converter component.
    /// </summary>
    public class XmlHttpConverterOptions : IOptions<XmlHttpConverterOptions>
    {
        /// <summary>
        /// Gets or sets the property name for the link location. 
        /// Should be set to null for HTTP header location.
        /// </summary>
        public string LinkLocation { get; set; }
        /// <summary>
        /// Returns this.
        /// </summary>
        public XmlHttpConverterOptions Value => this;
        /// <summary>
        /// Returns this.
        /// </summary>
        public IOptions<XmlHttpConverterOptions> GetOptions() => this;
        /// <summary>
        /// Sets the element name for the collection of links.
        /// </summary>
        /// <param name="elementName">The element name for the link location.</param>
        /// <returns>The current instance.</returns>
        public XmlHttpConverterOptions UseLinkLocation(string elementName)
        {
            LinkLocation = elementName;
            return this;
        }
        /// <summary>
        /// Sets the location for the links to the HTTP header area.
        /// </summary>
        /// <returns>The current instance.</returns>
        public XmlHttpConverterOptions UseLinksInHeaders()
        {
            LinkLocation = null;
            return this;
        }
    }
}
