using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Helper class for extension methods.
    /// </summary>
    public static class RestForAspNetCoreExt
    {
        /// <summary>
        /// Adds an RestForAspNetCore middleware component to an application pipeline.
        /// </summary>
        /// <param name="app">The application builder/</param>
        /// <returns>An application builder.</returns>
        public static IApplicationBuilder UseRest(this IApplicationBuilder app)
        {
            {   // Prime the schema cache:
                var repositories = app.ApplicationServices.GetServices<IRestRepository>();
                var typeDescriptorCreator = app.ApplicationServices.GetRequiredService<TypeDescriptorCreator>();
                foreach (var type in repositories.SelectMany(repo => repo.SchemaTypes).Distinct())
                    typeDescriptorCreator.GetDescriptor(type);
                typeDescriptorCreator.GetDescriptor(typeof(TypeDescriptor));
                typeDescriptorCreator.GetDescriptor(typeof(RestCapabilities));

                // Prime attribute based rest identity provider:
                var idProv = app.ApplicationServices.GetService<IRestIdentityProvider>() as DefaultAspRestIdentityProvider;
                idProv?.Initialize(repositories, app.ApplicationServices.GetServices<IRestPathMapping>());
            }

            return app.UseMiddleware<RestForAspNetCore>();
        }
        /// <summary>
        /// Adds services for RestForAspNetCore to the specified service collection.
        /// RestForAspNetCore depends on the IHttpContextAccessor implementation for its security implementation,
        /// and registers the HttpContextAccessor class as implementation.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add RestForAspNetCore to.</param>
        /// <param name="builder">A Rest Services Builder.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRest(this IServiceCollection serviceCollection, Action<IRestServicesBuilder> builder = null)
        {
            serviceCollection.AddSingleton<CoreRestRequestHandler>();
            serviceCollection.AddSingleton<TypeDescriptorCreator>();
            serviceCollection.AddRestRepository<SchemaRepository>();
            serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            serviceCollection.AddTransient<IUser, AspNetCoreUser>();
            serviceCollection.AddScoped(sp => sp.GetRequiredService<IRestIdentityProvider>().Prefixes.Copy());

            var restbuilder = new RestServicesBuilder(serviceCollection);
            builder?.Invoke(restbuilder);
            restbuilder.EndConfiguration();

            if (!serviceCollection.Any(sd => sd.ServiceType == typeof(IAuthorizationProvider)))
                serviceCollection.AddScoped<IAuthorizationProvider, AlwaysAllowAuthorizationProvider>();
            if (!serviceCollection.Any(sd => sd.ServiceType == typeof(IRestIdentityProvider)))
                throw new InvalidOperationException("Rest component depends on an IRestIdentityProvider implementation.");
            return serviceCollection;
        }

        private class RestServicesBuilder : IRestServicesBuilder
        {
            public Func<IServiceProvider, IHttpRestRequestHandler, IHttpRestRequestHandler> httpHandlerConfigurator;
            public Func<IServiceProvider, IRestRequestHandlerBuilder, IRestRequestHandlerBuilder> restHandlerConfigurator;
            public Action<IServiceCollection> endConfigurator = sc => { };

            public RestServicesBuilder(IServiceCollection serviceCollection)
            {
                ServiceCollection = serviceCollection;
                httpHandlerConfigurator = (sp, x) => x;
                restHandlerConfigurator = (sp, x) => x;
            }
            public IServiceCollection ServiceCollection { get; }

            public void EndConfiguration()
            {
                endConfigurator(ServiceCollection);
                ServiceCollection.AddSingleton(sp => httpHandlerConfigurator(sp, HttpRestRequestHandler.Create()));
                ServiceCollection.AddSingleton(sp => restHandlerConfigurator(sp, RestRequestHandlerBuilder.Create())
                    .Run(() => sp.GetRequiredService<CoreRestRequestHandler>().HandleRequest));
            }
            private Func<T, T> Compose<T>(Func<T, T> f, Func<T, T> g) => x => f(g(x));
            private Func<X, T, T> Compose<X, T>(Func<X, T, T> f, Func<X, T, T> g) => (x, y) => f(x, g(x, y));

            public IRestServicesBuilder UseHttpRequestHandler(Func<IServiceProvider, IHttpRestRequestHandler, IHttpRestRequestHandler> configurator)
            {
                httpHandlerConfigurator = Compose(configurator, httpHandlerConfigurator);
                return this;
            }

            public IRestServicesBuilder UseRequestHandler(Func<IServiceProvider, IRestRequestHandlerBuilder, IRestRequestHandlerBuilder> configurator)
            {
                restHandlerConfigurator = Compose(configurator, restHandlerConfigurator);
                return this;
            }

            public void OnEndConfiguration(Action<IServiceCollection> endConfigurator)
            {
                var old = this.endConfigurator;
                this.endConfigurator = sc => { old(sc); endConfigurator(sc); };
            }
        }
    }
}
