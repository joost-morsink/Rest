using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// This default Html generator is for testing purposes only.
    /// </summary>
    public class DefaultHtmlGenerator : AbstractGeneralHtmlGenerator
    {
        private readonly HtmlRestSerializer htmlRestSerializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="generatorProvider">A provider for specific Html generators.</param>
        public DefaultHtmlGenerator(ISpecificHtmlGeneratorProvider generatorProvider,HtmlRestSerializer htmlRestSerializer) : base(generatorProvider)
        {
            this.htmlRestSerializer = htmlRestSerializer;
        }
        /// <summary>
        /// Uses the HtmlRestSerializer class for generic generation of tables for data items.
        /// </summary>
        /// <param name="restValue">A Rest value.</param>
        /// <returns>An Html representation of the specified Rest value.</returns>
        protected override string DefaultHandleValue(IRestValue restValue)
        {
            var item = htmlRestSerializer.Serialize(restValue);
            return htmlRestSerializer.ToHtmlPage(item);
            
        }
    }
}
