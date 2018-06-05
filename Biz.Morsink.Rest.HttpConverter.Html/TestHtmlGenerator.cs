using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    public class TestHtmlGenerator : AbstractGeneralHtmlGenerator
    {
        public TestHtmlGenerator(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string DefaultHandleSuccess(IRestValue restValue)
        {
            var gen = (ISpecificHtmlGenerator)Activator.CreateInstance(typeof(TableGenerator<>).MakeGenericType(restValue.ValueType));
            return gen.GenerateHtml(restValue);
        }

        protected override string HandleFailure(IRestFailure f)
        {
            var sb = new StringBuilder();
            sb.Append("<h1>");
            sb.Append(f.Reason);
            sb.Append("</h1>");
            if(f is IHasRestValue hrv && hrv.RestValue is RestValue<ExceptionInfo> rvei)
            {
                sb.Append("<h2>");
                sb.Append(rvei.Value.Type);
                sb.Append("</h2><h3>");
                sb.Append(rvei.Value.Message);
                sb.Append("</h3>");
            }
            return sb.ToString();
        }

        protected override string HandlePending(IRestPending p)
        {
            if(p is IHasRestValue hrv)
            {
                return DefaultHandleSuccess(hrv.RestValue);
            } else
            {
                return $"<h1>Pending</h1>";
            }

        }

        protected override string HandleRedirect(IRestRedirect r)
        {
            return "";
        }
    }
}
