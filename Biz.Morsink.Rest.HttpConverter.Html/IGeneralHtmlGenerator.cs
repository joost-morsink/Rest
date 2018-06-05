using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    public interface IGeneralHtmlGenerator
    {
        string GenerateHtml(IRestResult result);
    }
}
