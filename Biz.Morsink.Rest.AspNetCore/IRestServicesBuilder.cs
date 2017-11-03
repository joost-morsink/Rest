using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// A decorating interface over IServiceCollection to apply Rest related configuration.
    /// </summary>
    public interface IRestServicesBuilder
    {
        /// <summary>
        /// The underlying IServiceCollection.
        /// </summary>
        IServiceCollection ServiceCollection { get; }
        /// <summary>
        /// Adds components to the Rest HTTP request pipeline.
        /// </summary>
        /// <param name="configurator">A configurator function for the Rest HTTP pipeline</param>
        /// <returns>The builder.</returns>
        IRestServicesBuilder UseHttpRequestHandler(Func<IServiceProvider, IHttpRestRequestHandler, IHttpRestRequestHandler> configurator);
        /// <summary>
        /// Adds components to the Rest request handler pipeline.
        /// </summary>
        /// <param name="configurator">A configurator function for the Rest request handler pipeline.</param>
        /// <returns>The builder.</returns>
        IRestServicesBuilder UseRequestHandler(Func<IServiceProvider, IRestRequestHandlerBuilder, IRestRequestHandlerBuilder> configurator);
        /// <summary>
        /// Ends configuration for this IRestServicesBuilder. 
        /// Should construct the Rest HTTP pipeline and request handler pipeline and add them to the ServiceCollection.
        /// </summary>
        void EndConfiguration();
        void OnEndConfiguration(Action<IServiceCollection> endConfigurator);
    }
    /// <summary>
    /// Helper extension methods for IRestServicesBuilder
    /// </summary>
    public static class RestServicesBuilderExt
    {
        /// <summary>
        /// Adds components to the Rest HTTP pipeline.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder.</param>
        /// <param name="configurator">A configurator function for the Rest HTTP pipeline</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder UseHttpRequestHandler(this IRestServicesBuilder builder, Func<IHttpRestRequestHandler, IHttpRestRequestHandler> configurator)
            => builder.UseHttpRequestHandler((sp, x) => configurator(x));
        /// <summary>
        /// Adds components to the Rest request handler pipeline.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder.</param>
        /// <param name="configurator">A configurator function for the Rest request handler pipeline.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder UseRequestHandler(this IRestServicesBuilder builder, Func<IRestRequestHandlerBuilder, IRestRequestHandlerBuilder> configurator)
            => builder.UseRequestHandler((sp, x) => configurator(x));
    }
}
