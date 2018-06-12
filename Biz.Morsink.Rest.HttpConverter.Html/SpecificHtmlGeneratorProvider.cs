using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// An implementation of ISpecificHtmlGeneratorProvider based on an IServiceProvider.
    /// </summary>
    public class SpecificHtmlGeneratorProvider : ISpecificHtmlGeneratorProvider
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SpecificHtmlGeneratorProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Gets a specific Html generator for a specific type.
        /// </summary>
        /// <typeparam name="T">A type to get a specific Html generator for.</typeparam>
        /// <returns>A specific Html generator if one can be provider, null otherwise.</returns>
        public ISpecificHtmlGenerator<T> GetGenerator<T>()
            => serviceProvider.GetService(typeof(ISpecificHtmlGenerator<T>)) as ISpecificHtmlGenerator<T>;
        /// <summary>
        /// Gets a specific Html generator for a specific type.
        /// </summary>
        /// <param name="type">A type to get a specific Html generator for.</param>
        /// <returns>A specific Html generator if one can be provider, null otherwise.</returns>
        public ISpecificHtmlGenerator GetGeneratorForType(Type type)
            => serviceProvider.GetService(typeof(ISpecificHtmlGenerator<>).MakeGenericType(type)) as ISpecificHtmlGenerator;
    }
}
