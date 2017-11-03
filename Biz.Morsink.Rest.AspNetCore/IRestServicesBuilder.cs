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
        /// <summary>
        /// Registers an Action to execute when the EndConfiguration method is called.
        /// This callback is allowed to inspect the service collection, and if necessary register extra or default components.
        /// </summary>
        /// <param name="endConfigurator">The Action./param>
        void OnEndConfiguration(Action<IServiceCollection> endConfigurator);
    }
  
}
