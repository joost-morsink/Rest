using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// Interface for general Html generators.
    /// </summary>
    public interface IGeneralHtmlGenerator
    {
        /// <summary>
        /// Generates an Html representation for a Rest result.
        /// </summary>
        /// <param name="result">A Rest result.</param>
        /// <returns>An Html representation for the Rest result.</returns>
        string GenerateHtml(IRestResult result);
    }
}
