using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// Interface for specific Html generators.
    /// </summary>
    public interface ISpecificHtmlGenerator
    {
        /// <summary>
        /// Contains the type which the instance is an Html generator for.
        /// </summary>
        Type ForType { get; }
        /// <summary>
        /// Generates Html for a Rest value.
        /// </summary>
        /// <param name="value">A Rest value.</param>
        /// <returns>An Html representation of the Rest value.</returns>
        string GenerateHtml(IRestValue value);
    }
    /// <summary>
    /// Typed interface for specific Html generators.
    /// </summary>
    /// <typeparam name="T">The value type of the Rest value.</typeparam>
    public interface ISpecificHtmlGenerator<T> : ISpecificHtmlGenerator
    {
        /// <summary>
        /// Generates Html for a Rest value.
        /// </summary>
        /// <param name="value">A Rest value.</param>
        /// <returns>An Html representation of the Rest value.</returns>
        string GenerateHtml(RestValue<T> value);
    }
}
