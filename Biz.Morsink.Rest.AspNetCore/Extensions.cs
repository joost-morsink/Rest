﻿using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.Schema;
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
        /// Adds a Rest structure to the service collection.
        /// </summary>
        /// <typeparam name="S">The structure type.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="lifetime">The lifetime scope of the root type.</param>
        /// <returns>The service collection with added registrations.</returns>
        public static IServiceCollection AddRestStructure<S>(this IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where S : IRestStructure, new()
            => serviceCollection.AddRestStructure(new S(), lifetime);
        /// <summary>
        /// Adds a Rest structure to the service collection.
        /// </summary>
        /// <typeparam name="S">The structure type.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="structure">The structure.</param>
        /// <param name="lifetime">The lifetime scope of the root type.</param>
        /// <returns>The service collection with added registrations.</returns>
        public static IServiceCollection AddRestStructure<S>(this IServiceCollection serviceCollection, S structure, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where S : IRestStructure
        {
            structure.RegisterComponents(serviceCollection, lifetime);
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
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping(this IRestServicesBuilder builder, IRestPathMapping mapping)
        {
            builder.ServiceCollection.AddSingleton(mapping);
            return builder;
        }
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The type the mapping is for.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="componentTypes">The component types of the identity value.</param>
        /// <param name="wildcardType">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T>(this IRestServicesBuilder builder, string path, Type[] componentTypes = null, Type wildcardType = null)
            => builder.AddPathMapping(new RestPathMapping(typeof(T), path, componentTypes, wildcardType));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="type">The type the mapping is for.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="componentTypes">The component types of the identity value.</param>
        /// <param name="wildcardType">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping(this IRestServicesBuilder builder, Type type, string path, Type[] componentTypes = null, Type wildcardType = null)
            => builder.AddPathMapping(new RestPathMapping(type, path, componentTypes, wildcardType));

        /// <summary>
        /// Adds a structure to the service collection.
        /// </summary>
        /// <typeparam name="S">The structure type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="lifetime">The lifetime scope of the root type.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddStructure<S>(this IRestServicesBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where S : IRestStructure, new()
            => builder.AddStructure(new S(), lifetime);
        /// <summary>
        /// Adds a structure to the service collection.
        /// </summary>
        /// <typeparam name="S">The structure type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="structure">The structure.</param>
        /// <param name="lifetime">The lifetime scope of the root type.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddStructure<S>(this IRestServicesBuilder builder, S structure, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where S : IRestStructure
        {
            builder.ServiceCollection.AddRestStructure<S>(structure, lifetime);
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

            builder.ServiceCollection.Add(new ServiceDescriptor(typeof(IDynamicLinkProvider<>).MakeGenericType(ct), typeof(RestCollectionLinks<,,>).MakeGenericType(ct, et, et), ServiceLifetime.Scoped));
            return builder;
        }
        /// <summary>
        /// Adds the specified IRestIdentityProvider to the service collection.
        /// </summary>
        /// <typeparam name="T">The concrete type of the identity provider.</typeparam>
        /// <param name="builder">An IRestServicesBuilder.</param>
        /// <param name="lifetime">The lifetime scope to apply to the identity provider.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddIdentityProvider<T>(this IRestServicesBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where T : IRestIdentityProvider
        {
            builder.ServiceCollection.Add(new ServiceDescriptor(typeof(IRestIdentityProvider), typeof(T), lifetime));
            return builder;
        }
        /// <summary>
        /// Adds the necessary components to support handling of the HTTP Options method.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddOptionsHandler(this IRestServicesBuilder builder)
            => builder.UseHttpRequestHandler(bld => bld.UseCapabilityDiscovery())
                .UseRequestHandler((sp, bld) => bld.Use<OptionsRequestHandler>(sp));
        /// <summary>
        /// Adds the necessary components to support handling of Caching metadata/headers.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddCaching(this IRestServicesBuilder builder)
            => builder
                .UseHttpRequestHandler(bld => bld.UseCaching())
                .UseRequestHandler((sp, bld) => bld.Use<CacheVersionTokenHandler>(sp));
        /// <summary>
        /// Adds the necessary components to support handling of Caching metadata/headers, as well as a cache implementation.
        /// </summary>
        /// <typeparam name="C">The type of the cache.</typeparam>
        /// <param name="builder">An IRestServicesBuilder</param>
        /// <param name="lifetime">The lifetime scope for the cache.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddCache<C>(this IRestServicesBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where C : IRestCache
        {
            builder.ServiceCollection.Add(new ServiceDescriptor(typeof(IRestCache), typeof(C), lifetime));

            return builder.AddCaching()
                .UseRequestHandler((sp, bld) => bld.Use<CacheRequestHandler>(sp));
        }

        /// <summary>
        /// Adds the necessary components to support handling of the Location metadatum/header.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddLocationHeader(this IRestServicesBuilder builder)
            => builder.UseHttpRequestHandler((sp, bld) => bld.UseLocationHeader(sp));
        /// <summary>
        /// Shortcut method for inserting various default services.
        /// Currently, OptionsHandler, Caching and Location are included.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddDefaultServices(this IRestServicesBuilder builder)
            => builder.AddCaching().AddLocationHeader().AddOptionsHandler();
        /// <summary>
        /// Adds a default identity provider to the service collection.
        /// The default provider is able to inspect Rest repositories for attributes and it uses IRestPathMappings.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder implementation.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddDefaultIdentityProvider(this IRestServicesBuilder builder)
        {
            builder.ServiceCollection.AddSingleton<IRestIdentityProvider, DefaultAspRestIdentityProvider>();
            return builder;
        }
        /// <summary>
        /// Adds a Rest job store to the service collection.
        /// </summary>
        /// <typeparam name="S">The type of the job store.</typeparam>
        /// <param name="builder">An IRestServicesBuilder implementation.</param>
        /// <param name="lifetime">The lifetime scope of the store.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddJobStore<S>(this IRestServicesBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            builder.ServiceCollection.Add(new ServiceDescriptor(typeof(IRestJobStore), typeof(S), lifetime));
            return builder;
        }
        /// <summary>
        /// Adds a Rest job store to the service collection.
        /// </summary>
        /// <typeparam name="S">The type of the job store.</typeparam>
        /// <param name="builder">An IRestServicesBuilder implementation.</param>
        /// <param name="factory">A factory method that constructs the store.</param>
        /// <param name="lifetime">The lifetime scope of the store.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddJobStore<S>(this IRestServicesBuilder builder, Func<IServiceProvider, S> factory, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where S : class
        {
            builder.ServiceCollection.Add(new ServiceDescriptor(typeof(IRestJobStore), factory, lifetime));
            return builder;
        }
        /// <summary>
        /// Adds RestJobs as a complete structure to the service collection and registers a request handler to catch long running requests.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder implementation.</param>
        /// <param name="jobRepositoryPath">The Rest path for the Job repository.</param>
        /// <param name="jobResultPathSuffix">A suffix to the job repository path for the result repository.</param>
        /// <param name="requestTimeout">A TimeSpan indicating how long a response might take before turning it into a pending one. Default 10 seconds.</param>
        /// <returns></returns>
        public static IRestServicesBuilder AddJobs(this IRestServicesBuilder builder, string jobRepositoryPath = "/job/*", string jobResultPathSuffix = "/result", TimeSpan? requestTimeout = null)
        {
            builder.UseRequestHandler((sp, p) => p.Use<ResponsePendingRequestHandler>(sp, requestTimeout ?? TimeSpan.FromSeconds(10.0)))
                .AddRepository<JobRepository>()
                .AddRepository<JobResultRepository>()
                .AddPathMapping<RestJob>(jobRepositoryPath)
                .AddPathMapping<RestJobResult>(jobRepositoryPath + jobResultPathSuffix)
                ;
            builder.ServiceCollection.AddTransient<ITypeRepresentation, RestJobRepresentation>();
            builder.OnEndConfiguration(sc =>
            {
                if (!sc.Any(sd => sd.ServiceType == typeof(IRestJobStore)))
                    sc.AddSingleton<IRestJobStore, MemoryRestJobStore>(sp => new MemoryRestJobStore(sp.GetRequiredService<IRestIdentityProvider>()));
            });
            builder.AddPathMapping<RestJobController>(jobRepositoryPath + "/controller/*", new[] { typeof(RestJob), typeof(RestJobController) });
            return builder;
        }
    }
}
