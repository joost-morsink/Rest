using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    public abstract class AbstractSpecificHtmlGenerator<T> : ISpecificHtmlGenerator<T>
    {
        public abstract string GenerateHtml(RestValue<T> value);

        string ISpecificHtmlGenerator.GenerateHtml(IRestValue value)
            => GenerateHtml((RestValue<T>)value);
    }
}
