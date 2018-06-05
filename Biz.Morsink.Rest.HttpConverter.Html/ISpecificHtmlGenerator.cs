using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    public interface ISpecificHtmlGenerator
    {
        string GenerateHtml(IRestValue value);
    }
    public interface ISpecificHtmlGenerator<T> : ISpecificHtmlGenerator
    {
        string GenerateHtml(RestValue<T> value);
    }
}
