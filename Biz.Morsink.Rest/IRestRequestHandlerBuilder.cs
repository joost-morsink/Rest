using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public interface IRestRequestHandlerBuilder
    {
        IRestRequestHandlerBuilder Use(Func<IRestRequestHandler, IRestRequestHandler> handler);
        IRestRequestHandler Run(Func<IRestRequestHandler> handler);
    }
    public static class RestRequestHandlerBuilderExt {
        public static IRestRequestHandlerBuilder Use(this IRestRequestHandlerBuilder builder, Func<IRestRequestHandler, Func<RestRequest, ValueTask<IRestResult>>> handler)
            => builder.Use(next => new FunctionalRestRequestHandler(handler(next)));
        private class FunctionalRestRequestHandler : IRestRequestHandler
        {
            private readonly Func<RestRequest, ValueTask<IRestResult>> f;

            public FunctionalRestRequestHandler(Func<RestRequest, ValueTask<IRestResult>> f)
            {
                this.f = f;
            }

            public ValueTask<IRestResult> HandleRequest(RestRequest request)
                => f(request);
        }
        public static IRestRequestHandlerBuilder Use<T>(this IRestRequestHandlerBuilder builder, IServiceLocator locator)
            where T : IRestRequestHandler
            =>  builder.Use(next =>
                {
                    var ctor = typeof(T).GetTypeInfo().DeclaredConstructors.First();
                    var parameters = ctor.GetParameters().Select(p => p.ParameterType == typeof(IRestRequestHandler) ? next : locator.ResolveRequired(p.ParameterType)).ToArray();
                    return (IRestRequestHandler)Activator.CreateInstance(typeof(T), parameters);
                });
        
    }
}
