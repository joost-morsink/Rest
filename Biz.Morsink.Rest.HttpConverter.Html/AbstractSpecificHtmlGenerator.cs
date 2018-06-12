using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// Abstract base class for specific Html generators.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractSpecificHtmlGenerator<T> : ISpecificHtmlGenerator<T>
    {
        /// <summary>
        /// Contains the type which the instance is an Html generator for.
        /// </summary>
        public Type ForType => typeof(T);
        /// <summary>
        /// Should generate an Html representation for a specific Rest value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract string GenerateHtml(RestValue<T> value);

        /// <summary>
        /// Generates an Html representation for a Rest value.
        /// </summary>
        /// <param name="value">A Rest value.</param>
        /// <returns>An Html representation for the specified Rest value.</returns>
        string ISpecificHtmlGenerator.GenerateHtml(IRestValue value)
            => GenerateHtml((RestValue<T>)value);
    }
}
