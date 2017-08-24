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
        IRestRequestHandlerBuilder Use(Func<RestRequestHandlerDelegate, RestRequestHandlerDelegate> handler);

        IRestRequestHandler Run(Func<RestRequestHandlerDelegate> handler);
    }
    public static class RestRequestHandlerBuilderExt {
        public static IRestRequestHandlerBuilder Use<T>(this IRestRequestHandlerBuilder builder, IServiceLocator locator)
            where T : IRestRequestHandler
            =>  builder.Use(next =>
                {
                    var ctor = typeof(T).GetTypeInfo().DeclaredConstructors.First();
                    var parameters = ctor.GetParameters().Select(p => p.ParameterType == typeof(RestRequestHandlerDelegate) ? next : locator.ResolveRequired(p.ParameterType)).ToArray();
                    return ((IRestRequestHandler)Activator.CreateInstance(typeof(T), parameters)).HandleRequest;
                });
        
    }
}
