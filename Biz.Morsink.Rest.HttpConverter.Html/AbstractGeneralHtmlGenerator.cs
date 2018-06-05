using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    public abstract class AbstractGeneralHtmlGenerator : IGeneralHtmlGenerator
    {
        private readonly IServiceProvider serviceProvider;

        public AbstractGeneralHtmlGenerator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

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

        protected abstract string HandlePending(IRestPending p);
        protected abstract string HandleRedirect(IRestRedirect r);
        protected abstract string HandleFailure(IRestFailure f);
        protected abstract string DefaultHandleSuccess(IRestValue restValue);

        protected virtual string HandleSuccess(IRestSuccess s)
        {
            if (TryHandleValue(s.RestValue, out var result))
                return result;
            else
                return DefaultHandleSuccess(s.RestValue);
        }
        
        protected virtual bool TryHandleValue(IRestValue restValue, out string result)
        {
            var generator = (ISpecificHtmlGenerator)serviceProvider.GetService(typeof(ISpecificHtmlGenerator<>).MakeGenericType(restValue.ValueType));
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
