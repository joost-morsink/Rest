using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    public class StaticHtmlGenerator : IGeneralHtmlGenerator
    {
        private readonly string content;

        public StaticHtmlGenerator(string content)
        {
            this.content = content;
        }
        public string GenerateHtml(IRestResult result)
            => content;
    }
}
