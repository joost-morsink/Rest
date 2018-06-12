using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// This interface specifies a provider pattern for specific Html generators.
    /// </summary>
    public interface ISpecificHtmlGeneratorProvider
    {
        /// <summary>
        /// Gets a specific Html generator for a specific type.
        /// </summary>
        /// <param name="type">A type to get a specific Html generator for.</param>
        /// <returns>A specific Html generator if one can be provider, null otherwise.</returns>
        ISpecificHtmlGenerator GetGeneratorForType(Type type);
        /// <summary>
        /// Gets a specific Html generator for a specific type.
        /// </summary>
        /// <typeparam name="T">A type to get a specific Html generator for.</typeparam>
        /// <returns>A specific Html generator if one can be provider, null otherwise.</returns>
        ISpecificHtmlGenerator<T> GetGenerator<T>();
    }
}
