using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A builder for a RestRequestHandler
    /// </summary>
    public interface IRestRequestHandlerBuilder
    {
        /// <summary>
        /// Inserts a handler into the pipeline.
        /// </summary>
        /// <param name="handler">A RequestHandlerDelegate decoration function. This function serves as a middleware component.</param>
        /// <returns>A builder using the specified decoration function.</returns>
        IRestRequestHandlerBuilder Use(Func<RestRequestHandlerDelegate, RestRequestHandlerDelegate> handler);

        /// <summary>
        /// Inserts the terminal handler into the pipeline.
        /// </summary>
        /// <param name="handler">A function producing the core RestRequestHandlerDelegate.</param>
        /// <returns>An instance of the IRestRequestHandler interface that uses all specified middleware and the specified terminal handler.</returns>
        IRestRequestHandler Run(Func<RestRequestHandlerDelegate> handler);
    }
    /// <summary>
    /// Extension methods for the RestRequest handler builder.
    /// </summary>
    public static class RestRequestHandlerBuilderExt
    {
        /// <summary>
        /// Uses an object of type T as a middleware component.
        /// The locator parameter is used to construct T's dependencies.
        /// This method can be used before the service locator is fully configured, but the IRestRequestHandler can only be constructed after the service locator is fully configured.
        /// </summary>
        /// <typeparam name="T">The type of middleware component.</typeparam>
        /// <param name="builder">The builder to add the middleware component to.</param>
        /// <param name="locator">A service locator to resolve all the middleware's dependencies.</param>
        /// <param name="fixedParameters">Fixed parameters for the constructor of the middleware component.</param>
        /// <returns>A new builder using the middleware component.</returns>
        public static IRestRequestHandlerBuilder Use<T>(this IRestRequestHandlerBuilder builder, IServiceProvider locator, params object[] fixedParameters)
            where T : IRestRequestHandler
            => builder.Use(next =>
               {
                   var ctor = typeof(T).GetTypeInfo().DeclaredConstructors.First();
                   var ctorParams = ctor.GetParameters();
                   var parameters = ctorParams.TakeWhile(p => p.ParameterType == typeof(RestRequestHandlerDelegate)).Select(p => (object)next)
                       .Concat(ctorParams.SkipWhile(p => p.ParameterType == typeof(RestRequestHandlerDelegate)).Select(
                           (p, idx) => idx < fixedParameters.Length
                               ? fixedParameters[idx]
                               : locator.GetService(p.ParameterType)))
                           .ToArray();

                   return ((IRestRequestHandler)Activator.CreateInstance(typeof(T), parameters)).HandleRequest;
               });

    }
}
