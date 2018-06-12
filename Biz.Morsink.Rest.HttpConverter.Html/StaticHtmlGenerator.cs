using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// A general Html generator that always generates the same (static) content.
    /// </summary>
    public class StaticHtmlGenerator : IGeneralHtmlGenerator
    {
        private readonly string content;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="content">The content to generate.</param>
        public StaticHtmlGenerator(string content)
        {
            this.content = content;
        }
        /// <summary>
        /// Generates a static document, ignoring the result value.
        /// </summary>
        public string GenerateHtml(IRestResult result)
            => content;
    }
}
