using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// This default Html generator is for testing purposes only.
    /// </summary>
    public class DefaultHtmlGenerator : AbstractGeneralHtmlGenerator
    {
        private readonly IEnumerable<ITypeRepresentation> typeRepresentations;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="generatorProvider">A provider for specific Html generators.</param>
        public DefaultHtmlGenerator(ISpecificHtmlGeneratorProvider generatorProvider, IEnumerable<ITypeRepresentation> typeRepresentations) : base(generatorProvider)
        {
            this.typeRepresentations = typeRepresentations;
        }
        /// <summary>
        /// Uses the TableGenerator&lt;T&gt; class for generic generation of tables for data items.
        /// </summary>
        /// <param name="restValue">A Rest value.</param>
        /// <returns>An Html representation of the specified Rest value.</returns>
        protected override string DefaultHandleValue(IRestValue restValue)
        {
            var gen = (ISpecificHtmlGenerator)Activator.CreateInstance(typeof(TableGenerator<>).MakeGenericType(restValue.ValueType), typeRepresentations);
            return gen.GenerateHtml(restValue);
        }
    }
}
