using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// Interface for building an HtmlHttpConverter.
    /// </summary>
    public interface IHtmlHttpConverterBuilder
    {
        /// <summary>
        /// The underlying IServiceCollection.
        /// </summary>
        IServiceCollection ServiceCollection { get; }
    }
}
