using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// This class contains extension methods for ASP.Net Core configuration.
    /// </summary>
    public static class Extensions
    {
        private static Type GetGeneric(this Type type, Type interf)
            => type.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GetGenericArguments().Length == 1 && i.GetGenericTypeDefinition() == interf)
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();

        /// <summary>
        /// This method adds a repository to the service collection as IRestRepository and IRestRepository&lt;T&gt;.
        /// </summary>
        /// <typeparam name="R">The type of the Rest repository.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="lifetime">The lifetime scope of the repository.</param>
        /// <returns>The service collection with added registrations.</returns>
        public static IServiceCollection AddRestRepository<R>(this IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where R : IRestRepository
        {
            serviceCollection.Add(new ServiceDescriptor(typeof(IRestRepository), typeof(R), lifetime));
            serviceCollection.Add(new ServiceDescriptor(typeof(IRestRepository<>).MakeGenericType(typeof(R).GetGeneric(typeof(IRestRepository<>))), typeof(R), lifetime));
            return serviceCollection;
        }
        /// <summary>
        /// This method adds a repository to the service collection as IRestRepository and IRestRepository&lt;T&gt;.
        /// </summary>
        /// <typeparam name="R">The type of the Rest repository.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="lifetime">The lifetime scope of the repository.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddRepository<R>(this IRestServicesBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where R : IRestRepository
        {
            builder.ServiceCollection.AddRestRepository<R>(lifetime);
            return builder;
        }
        /// <summary>
        /// This methods adds two repositories (one for a collection, one for an entity), a rest resource collection and a dynamic link provider for the collection type.
        /// </summary>
        /// <typeparam name="C">The collection repository type.</typeparam>
        /// <typeparam name="E">The entity repository type.</typeparam>
        /// <typeparam name="S">The 'source': A rest resource collection for the collection-entity pair.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="collectionLifetime">The lifetime scope of the collection repository.</param>
        /// <param name="entityLifetime">The lifetime scope of the entity repository.</param>
        /// <param name="sourceLifetime">The lifetime scope of the 'source'.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddCollection<C, E, S>(this IRestServicesBuilder builder,
            ServiceLifetime collectionLifetime = ServiceLifetime.Scoped,
            ServiceLifetime entityLifetime = ServiceLifetime.Scoped,
            ServiceLifetime sourceLifetime = ServiceLifetime.Scoped)
            where C : IRestRepository
            where E : IRestRepository
        {
            var ct = typeof(C).GetGeneric(typeof(IRestRepository<>));
            var et = typeof(E).GetGeneric(typeof(IRestRepository<>));
            if (ct == null)
                throw new ArgumentException("Collection type cannot be found.");
            if (et == null)
                throw new ArgumentException("Entity type cannot be found.");
            if (!typeof(IRestResourceCollection<,>).MakeGenericType(ct, et).GetTypeInfo().IsAssignableFrom(typeof(S)))
                throw new ArgumentException("Source type does not implement the correct IRestResourceCollection.");
            builder.AddRepository<C>(collectionLifetime)
                .AddRepository<E>(entityLifetime)
                .ServiceCollection.Add(new ServiceDescriptor(typeof(IRestResourceCollection<,>).MakeGenericType(ct, et), typeof(S), sourceLifetime));

            builder.ServiceCollection.Add(new ServiceDescriptor(typeof(IDynamicLinkProvider<>).MakeGenericType(ct), typeof(RestCollectionLinks<,>).MakeGenericType(ct, et), ServiceLifetime.Scoped));
            return builder;
        }
        /// <summary>
        /// Configure the IRestHttpPipeline.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder</param>
        /// <param name="pipeline">The pipeline configurator.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder ConfigurePipeline(this IRestServicesBuilder builder, Func<IRestHttpPipeline, IRestHttpPipeline> pipeline)
        {
            builder.ServiceCollection.AddSingleton(pipeline(RestHttpPipeline.Create()));
            return builder;
        }
        /// <summary>
        /// Configure the IRestRequestHandler
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder</param>
        /// <param name="handlerBuilder">The handler configurator.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder ConfigureRequestHandler(this IRestServicesBuilder builder, Func<IRestRequestHandlerBuilder, IServiceProvider, IRestRequestHandlerBuilder> handlerBuilder)
        {
            builder.ServiceCollection.AddSingleton(sp =>
                handlerBuilder(RestRequestHandlerBuilder.Create(), sp)
                .Run(() => sp.GetRequiredService<CoreRestRequestHandler>().HandleRequest));
            return builder;
        }
        /// <summary>
        /// Adds the specified IRestIdentityProvider to the service collection.
        /// </summary>
        /// <typeparam name="T">The concrete type of the identity provider.</typeparam>
        /// <param name="builder">An IRestServicesBuilder.</param>
        /// <param name="lifetime">The lifetime scope to apply to the identity provider.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddIdentityProvider<T>(this IRestServicesBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where T : IRestIdentityProvider
        {
            builder.ServiceCollection.Add(new ServiceDescriptor(typeof(IRestIdentityProvider), typeof(T), lifetime));
            return builder;
        }
    }
}
