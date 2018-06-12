using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// Abstract base class for general Html generators.
    /// </summary>
    public abstract class AbstractGeneralHtmlGenerator : IGeneralHtmlGenerator
    {
        private readonly ISpecificHtmlGeneratorProvider generatorProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="generatorProvider">A provider for specific Html generators.</param>
        public AbstractGeneralHtmlGenerator(ISpecificHtmlGeneratorProvider generatorProvider)
        {
            this.generatorProvider = generatorProvider;
        }
        /// <summary>
        /// Generates Html for a given Rest result.
        /// </summary>
        /// <param name="result">The result to generate Html for.</param>
        /// <returns>An Html representation of the specified Rest result.</returns>
        public string GenerateHtml(IRestResult result)
        {
            switch (result)
            {
                case IRestSuccess s:
                    return HandleSuccess(s);
                case IRestFailure f:
                    return HandleFailure(f);
                case IRestRedirect r:
                    return HandleRedirect(r);
                case IRestPending p:
                    return HandlePending(p);
                default:
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// Handles 'pending' results.
        /// </summary>
        /// <param name="p">Then pending Rest result.</param>
        /// <returns>An Html representation of the specified pending Rest result.</returns>
        protected virtual string HandlePending(IRestPending p)
        {
            if (p is IHasRestValue hrv)
                return DefaultHandleValue(hrv.RestValue);
            else
                return $"<h1>Pending</h1>";
        }
        /// <summary>
        /// Handles redirection results.
        /// </summary>
        /// <param name="r">The redirect result.</param>
        /// <returns>Null, as redirections should have no content.</returns>
        protected virtual string HandleRedirect(IRestRedirect r) => null;
        /// <summary>
        /// Handles failure results.
        /// </summary>
        /// <param name="f">A failure Rest result.</param>
        /// <returns>An Html representation of the specified failure Rest result.</returns>
        protected virtual string HandleFailure(IRestFailure f)
        {
            var sb = new StringBuilder();
            sb.Append("<h1>");
            sb.Append(f.Reason);
            sb.Append("</h1>");
            if (f is IHasRestValue hrv && hrv.RestValue is RestValue<ExceptionInfo> rvei)
            {
                sb.Append("<h2>");
                sb.Append(rvei.Value.Type);
                sb.Append("</h2><h3>");
                sb.Append(rvei.Value.Message);
                sb.Append("</h3>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Abstract member for the default handling of Rest values.
        /// </summary>
        /// <param name="restValue">A Rest value.</param>
        /// <returns>An Html representation of the specified Rest value.</returns>
        protected abstract string DefaultHandleValue(IRestValue restValue);

        /// <summary>
        /// Handles 'success' results.
        /// </summary>
        /// <param name="s">a success Rest result.</param>
        /// <returns>An Html representation of the success Rest value.</returns>
        protected virtual string HandleSuccess(IRestSuccess s)
        { 
            if (TryHandleValue(s.RestValue, out var result))
                return result;
            else
                return DefaultHandleValue(s.RestValue);
        }
        
        /// <summary>
        /// Tries to handle the value by looking for a specific Html generator for the value's type.
        /// </summary>
        /// <param name="restValue">The Rest value to handle.</param>
        /// <param name="result">The resulting Html if the try is successful.</param>
        /// <returns>True if Html was successfully generated for the value, false otherwise.</returns>
        protected virtual bool TryHandleValue(IRestValue restValue, out string result)
        {
            var generator = generatorProvider.GetGeneratorForType(restValue.ValueType);
            if (generator == null)
            {
                result = null;
                return false;
            }
            else
            {
                result = generator.GenerateHtml(restValue);
                return true;
            }
        }
    }
}
