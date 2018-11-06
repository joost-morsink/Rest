using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.MediaTypes;
using Biz.Morsink.Rest.AspNetCore.Problem;
using Biz.Morsink.Rest.Jobs;
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
            var entityType = typeof(R).GetGeneric(typeof(IRestRepository<>));
            serviceCollection.Add(new ServiceDescriptor(typeof(IRestRepository), typeof(R), lifetime));
            serviceCollection.Add(new ServiceDescriptor(typeof(IRestRepository<>).MakeGenericType(entityType), typeof(R), lifetime));
            foreach (var attr in typeof(R).GetTypeInfo().GetCustomAttributes<RestPathAttribute>())
                serviceCollection.AddRestPathMapping(entityType, attr.Path, attr.ComponentTypes, attr.WildcardTypes);

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
        /// This method adds an attributed container's rest repositories to the service collection.
        /// </summary>
        /// <typeparam name="C">The 'parent' container's type.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="lifetime">The lifetime scope for the container (default = Scoped).</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddAttributedRestRepository<C>(this IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            serviceCollection.Add(new ServiceDescriptor(typeof(C), typeof(C), lifetime));
            foreach (var (key, f) in AttributedRestRepositories.GetRepositoryFactories((IServiceProvider sp) => sp.GetRequiredService<C>()))
            {
                serviceCollection.Add(new ServiceDescriptor(typeof(IRestRepository), f, lifetime));
                serviceCollection.Add(new ServiceDescriptor(typeof(IRestRepository<>).MakeGenericType(key), f, lifetime));
            }
            return serviceCollection;
        }
        /// <summary>
        /// This method adds an attributed container's rest repositories to the service collection.
        /// </summary>
        /// <typeparam name="C">The 'parent' container's type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="lifetime">The lifetime scope for the container (default = Scoped).</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddAttributedRepository<C>(this IRestServicesBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            builder.ServiceCollection.AddAttributedRestRepository<C>(lifetime);
            return builder;
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
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T>(this IRestServicesBuilder builder, string path, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(T), path, new[] { typeof(T) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T, U>(this IRestServicesBuilder builder, string path, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(U), path, new[] { typeof(T), typeof(U) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T, U, V>(this IRestServicesBuilder builder, string path, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(V), path, new[] { typeof(T), typeof(U), typeof(V) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <typeparam name="W">The fourth component type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T, U, V, W>(this IRestServicesBuilder builder, string path, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(W), path, new[] { typeof(T), typeof(U), typeof(V), typeof(W) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <typeparam name="W">The fourth component type.</typeparam>
        /// <typeparam name="X">The fifth component type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T, U, V, W, X>(this IRestServicesBuilder builder, string path, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(X), path, new[] { typeof(T), typeof(U), typeof(V), typeof(W), typeof(X) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The type the mapping is for.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T>(this IRestServicesBuilder builder, string path, Version version, Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(T), path, new[] { typeof(T) }, wildcardTypes, version));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T, U>(this IRestServicesBuilder builder, string path, Version version, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(U), path, new[] { typeof(T), typeof(U) }, wildcardTypes, version));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T, U, V>(this IRestServicesBuilder builder, string path, Version version, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(V), path, new[] { typeof(T), typeof(U), typeof(V) }, wildcardTypes, version));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <typeparam name="W">The fourth component type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T, U, V, W>(this IRestServicesBuilder builder, string path, Version version, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(W), path, new[] { typeof(T), typeof(U), typeof(V), typeof(W) }, wildcardTypes, version));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <typeparam name="W">The fourth component type.</typeparam>
        /// <typeparam name="X">The fifth component type.</typeparam>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping<T, U, V, W, X>(this IRestServicesBuilder builder, string path, Version version, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(typeof(X), path, new[] { typeof(T), typeof(U), typeof(V), typeof(W), typeof(X) }, wildcardTypes, version));

        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <param name="type">The type the mapping is for.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="componentTypes">The component types of the identity value.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddPathMapping(this IRestServicesBuilder builder, Type type, string path, Type[] componentTypes = null, params Type[] wildcardTypes)
            => builder.AddPathMapping(new RestPathMapping(type, path, componentTypes, wildcardTypes));

        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <param name="builder">An IServiceCollection instance.</param>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping(this IServiceCollection serviceCollection, IRestPathMapping mapping)
        {
            serviceCollection.AddSingleton(mapping);
            return serviceCollection;
        }
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The type the mapping is for.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping<T>(this IServiceCollection serviceCollection, string path, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(T), path, new[] { typeof(T) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping<T, U>(this IServiceCollection serviceCollection, string path, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(U), path, new[] { typeof(T), typeof(U) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping<T, U, V>(this IServiceCollection serviceCollection, string path, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(V), path, new[] { typeof(T), typeof(U), typeof(V) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <typeparam name="W">The fourth component type.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns> 
        public static IServiceCollection AddRestPathMapping<T, U, V, W>(this IServiceCollection serviceCollection, string path, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(W), path, new[] { typeof(T), typeof(U), typeof(V), typeof(W) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <typeparam name="W">The fourth component type.</typeparam>
        /// <typeparam name="X">The fifth component type.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping<T, U, V, W, X>(this IServiceCollection serviceCollection, string path, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(X), path, new[] { typeof(T), typeof(U), typeof(V), typeof(W), typeof(X) }, wildcardTypes));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The type the mapping is for.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping<T>(this IServiceCollection serviceCollection, string path, Version version, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(T), path, new[] { typeof(T) }, wildcardTypes, version));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping<T, U>(this IServiceCollection serviceCollection, string path, Version version, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(U), path, new[] { typeof(T), typeof(U) }, wildcardTypes, version));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping<T, U, V>(this IServiceCollection serviceCollection, string path, Version version, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(V), path, new[] { typeof(T), typeof(U), typeof(V) }, wildcardTypes, version));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <typeparam name="W">The fourth component type.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns> 
        public static IServiceCollection AddRestPathMapping<T, U, V, W>(this IServiceCollection serviceCollection, string path, Version version, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(W), path, new[] { typeof(T), typeof(U), typeof(V), typeof(W) }, wildcardTypes, version));
        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <typeparam name="T">The first component type.</typeparam>
        /// <typeparam name="U">The second component type.</typeparam>
        /// <typeparam name="V">The third component type.</typeparam>
        /// <typeparam name="W">The fourth component type.</typeparam>
        /// <typeparam name="X">The fifth component type.</typeparam>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="version">The version of the mapping.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping<T, U, V, W, X>(this IServiceCollection serviceCollection, string path, Version version, params Type[] wildcardTypes)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(typeof(X), path, new[] { typeof(T), typeof(U), typeof(V), typeof(W), typeof(X) }, wildcardTypes, version));

        /// <summary>
        /// Adds a path mapping to the service collection.
        /// </summary>
        /// <param name="serviceCollection">An IServiceCollection instance.</param>
        /// <param name="type">The type the mapping is for.</param>
        /// <param name="path">The path of the mapping.</param>
        /// <param name="componentTypes">The component types of the identity value.</param>
        /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
        /// <returns>The builder.</returns>
        public static IServiceCollection AddRestPathMapping(this IServiceCollection serviceCollection, Type type, string path, Type[] componentTypes = null, Type[] wildcardTypes = null)
            => serviceCollection.AddRestPathMapping(new RestPathMapping(type, path, componentTypes, wildcardTypes));

        /// <summary>
        /// Starts building rest path mappings for some path.
        /// </summary>
        /// <param name="serviceCollection">A service collection.</param>
        /// <param name="path">The path.</param>
        /// <returns>A RestPathMappingBuilder.</returns>
        public static RestPathMappingBuilder OnRestPath(this IServiceCollection serviceCollection, string path)
        {
            return new RestPathMappingBuilder(serviceCollection, path);
        }
        /// <summary>
        /// Starts building rest path mappings for some path.
        /// </summary>
        /// <param name="builder">A Rest services builder.</param>
        /// <param name="path">The path.</param>
        /// <returns>A RestPathMappingBuilder.</returns>
        public static RestPathMappingBuilder OnRestPath(this IRestServicesBuilder builder, string path)
        {
            return new RestPathMappingBuilder(builder.ServiceCollection, path);
        }
        /// <summary>
        /// Starts building rest path mappings for some path.
        /// </summary>
        /// <param name="serviceCollection">A service collection.</param>
        /// <param name="path">The path.</param>
        /// <param name="builderAction">An action to perform on the RestPathMappingBuilder.</param>
        /// <returns>the services collection.</returns>
        public static IServiceCollection OnRestPath(this IServiceCollection serviceCollection, string path, Action<RestPathMappingBuilder> builderAction)
        {
            builderAction(serviceCollection.OnRestPath(path));
            return serviceCollection;
        }
        /// <summary>
        /// Starts building rest path mappings for some path.
        /// </summary>
        /// <param name="builder">A Rest services builder.</param>
        /// <param name="path">The path.</param>
        /// <param name="builderAction">An action to perform on the RestPathMappingBuilder.</param>
        /// <returns>the services collection.</returns>
        public static IRestServicesBuilder OnPath(this IRestServicesBuilder builder, string path, Action<RestPathMappingBuilder> builderAction)
        {
            builderAction(builder.ServiceCollection.OnRestPath(path));
            return builder;
        }
        /// <summary>
        /// A builder pattern struct for RestPathMappings.
        /// </summary>
        public struct RestPathMappingBuilder
        {
            private readonly IServiceCollection services;
            private readonly string path;
            private readonly Version version;
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="services">A service collection.</param>
            /// <param name="path">The path to build entries for.</param>
            public RestPathMappingBuilder(IServiceCollection services, string path)
            {
                this.services = services;
                this.path = path;
                version = new Version(1, 0);
            }
            private RestPathMappingBuilder(IServiceCollection services, string path, Version version)
            {
                this.services = services;
                this.path = path;
                this.version = version;
            }
            /// <summary>
            /// Sets the version for the next addition.
            /// </summary>
            /// <param name="version">The version.</param>
            public RestPathMappingBuilder ForVersion(Version version)
                => new RestPathMappingBuilder(services, path, version);
            /// <summary>
            /// Sets the version for the next addition.
            /// </summary>
            /// <param name="major">The major version.</param>
            public RestPathMappingBuilder ForVersion(int major)
                => ForVersion(new Version(major, 0));
            /// <summary>
            /// Sets the version for the next addition.
            /// </summary>
            /// <param name="major">The major version.</param>
            /// <param name="minor">The minor version.</param>
            public RestPathMappingBuilder ForVersion(int major, int minor)
                => ForVersion(new Version(major, minor));
            /// <summary>
            /// Sets the version for the next addition.
            /// </summary>
            /// <param name="major">The major version.</param>
            /// <param name="minor">The minor version.</param>
            /// <param name="patch">The patch version.</param>
            public RestPathMappingBuilder ForVersion(int major, int minor, int patch)
                => ForVersion(new Version(major, minor, patch));
            /// <summary>
            /// Adds a Rest path mapping to the service collection.
            /// </summary>
            /// <typeparam name="T">The resource type.</typeparam>
            /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
            public RestPathMappingBuilder Add<T>(params Type[] wildcardTypes)
            {
                services.AddRestPathMapping<T>(path, version, wildcardTypes);
                return new RestPathMappingBuilder(services, path, new Version(version.Major, 0));
            }
            /// <summary>
            /// Adds a Rest path mapping to the service collection.
            /// </summary>
            /// <typeparam name="T">The first component type.</typeparam>
            /// <typeparam name="U">The second component type.</typeparam>
            /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
            public RestPathMappingBuilder Add<T, U>(params Type[] wildcardTypes)
            {
                services.AddRestPathMapping<T, U>(path, version, wildcardTypes);
                return new RestPathMappingBuilder(services, path, new Version(version.Major, 0));
            }
            /// <summary>
            /// Adds a Rest path mapping to the service collection.
            /// </summary>
            /// <typeparam name="T">The first component type.</typeparam>
            /// <typeparam name="U">The second component type.</typeparam>
            /// <typeparam name="V">The third component type.</typeparam>
            /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
            public RestPathMappingBuilder Add<T, U, V>(params Type[] wildcardTypes)
            {
                services.AddRestPathMapping<T, U, V>(path, version, wildcardTypes);
                return new RestPathMappingBuilder(services, path, new Version(version.Major, 0));
            }
            /// <summary>
            /// Adds a Rest path mapping to the service collection.
            /// </summary>
            /// <typeparam name="T">The first component type.</typeparam>
            /// <typeparam name="U">The second component type.</typeparam>
            /// <typeparam name="V">The third component type.</typeparam>
            /// <typeparam name="W">The fourth component type.</typeparam>
            /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
            public RestPathMappingBuilder Add<T, U, V, W>(params Type[] wildcardTypes)
            {
                services.AddRestPathMapping<T, U, V, W>(path, version, wildcardTypes);
                return new RestPathMappingBuilder(services, path, new Version(version.Major, 0));
            }
            /// <summary>
            /// Adds a Rest path mapping to the service collection.
            /// </summary>
            /// <typeparam name="T">The first component type.</typeparam>
            /// <typeparam name="U">The second component type.</typeparam>
            /// <typeparam name="V">The third component type.</typeparam>
            /// <typeparam name="W">The fourth component type.</typeparam>
            /// <typeparam name="X">The fifth component type.</typeparam>            
            /// <param name="wildcardTypes">An optional wildcard type for the query string.</param>
            public RestPathMappingBuilder Add<T, U, V, W, X>(params Type[] wildcardTypes)
            {
                services.AddRestPathMapping<T, U, V, W, X>(path, version, wildcardTypes);
                return new RestPathMappingBuilder(services, path, new Version(version.Major, 0));
            }

        }

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
        public static IRestServicesBuilder AddDefaultIdentityProvider(this IRestServicesBuilder builder, string localPrefix = null, params RestPrefix[] prefixes)
        {
            builder.ServiceCollection.AddSingleton<IRestIdentityProvider, DefaultAspRestIdentityProvider>(sp =>
            {
                var res = new DefaultAspRestIdentityProvider(localPrefix);
                foreach (var prefix in prefixes)
                    res.Prefixes.Register(prefix);
                return res;
            });
            return builder;
        }
        /// <summary>
        /// Adds an exception listener to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="action">The action to execute when an exception occurs.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection OnRestException(this IServiceCollection services, Action<Exception> action)
        {
            services.AddSingleton<IRestExceptionListener>(sp => new ExceptionListener(action));
            return services;
        }
        /// <summary>
        /// Adds an exception listener to the rest services.
        /// </summary>
        /// <param name="builder">A Rest services builder instance.</param>
        /// <param name="action">The action to execute when an exception occurs.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder OnException(this IRestServicesBuilder builder, Action<Exception> action)
        {
            builder.ServiceCollection.OnRestException(action);
            return builder;
        }
        /// <summary>
        /// Adds an exception listener to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="listener">The listener to notify when an exception occurs.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection OnRestException(this IServiceCollection services, IRestExceptionListener listener)
        {
            services.AddSingleton(listener);
            return services;
        }
        /// <summary>
        /// Adds an exception listener to the rest services.
        /// </summary>
        /// <param name="builder">A Rest services builder instance.</param>
        /// <param name="listener">The listener to notify when an exception occurs.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder OnException(this IRestServicesBuilder builder, IRestExceptionListener listener)
        {
            builder.ServiceCollection.OnRestException(listener);
            return builder;
        }
        /// <summary>
        /// Adds an exception listener to the service collection.
        /// </summary>
        /// <typeparam name="T">The type of listener to register for notification.</typeparam>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection OnRestException<T>(this IServiceCollection services)
            where T : class, IRestExceptionListener
        {
            services.AddSingleton<IRestExceptionListener, T>();
            return services;
        }
        /// <summary>
        /// Adds an exception listener to the rest services.
        /// </summary>
        /// <typeparam name="T">The type of listener to register for notification.</typeparam>
        /// <param name="builder">A Rest services builder instance.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder OnException<T>(this IRestServicesBuilder builder)
            where T : class, IRestExceptionListener
        {
            builder.ServiceCollection.OnRestException<T>();
            return builder;
        }
        private class ExceptionListener : IRestExceptionListener
        {
            private readonly Action<Exception> action;

            public ExceptionListener(Action<Exception> action)
            {
                this.action = action;
            }
            public void UnexpectedExceptionOccured(Exception ex)
                => action(ex);
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
        public static IRestServicesBuilder AddJobs(this IRestServicesBuilder builder, string jobRepositoryPath = "/job", string jobResultPathSuffix = "/result", TimeSpan? requestTimeout = null)
        {
            builder.UseRequestHandler((sp, p) => p.Use<ResponsePendingRequestHandler>(sp, requestTimeout ?? TimeSpan.FromSeconds(10.0)))
                .AddRepository<JobRepository>()
                .AddRepository<JobControllerRepository>()
                .AddRepository<JobResultRepository>()
                .AddRepository<JobCollectionRepository>()
                .AddRepository<JobFinishedRepository>()
                .AddPathMapping<RestJobCollection>(jobRepositoryPath + "?*")
                .AddPathMapping<RestJob>(jobRepositoryPath + "/*")
                .AddPathMapping<RestJob, RestJobController>(jobRepositoryPath + "/*/controller/*")
                .AddPathMapping<RestJob, RestJobController, RestJobFinished>(jobRepositoryPath + "/*/controller/*/finish+")
                .AddPathMapping<RestJobResult>(jobRepositoryPath + "/*" + jobResultPathSuffix)
                ;
            builder.ServiceCollection.AddTransient<ITypeRepresentation, RestJobRepresentation>();
            builder.ServiceCollection.AddTransient<ITypeRepresentation, RestJobControllerRepresentation>();
            builder.ServiceCollection.AddTransient<ITypeRepresentation, RestJobResultRepresentation>();
            builder.OnEndConfiguration(sc =>
            {
                if (!sc.Any(sd => sd.ServiceType == typeof(IRestJobStore)))
                    sc.AddSingleton<IRestJobStore, MemoryRestJobStore>(sp => new MemoryRestJobStore(sp.GetRequiredService<IRestIdentityProvider>()));
            });
            return builder;
        }
        /// <summary>
        /// Adds the OpenApiRepository endpoint to the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="path">The endpoint path for the OpenAPI Specification document.</param>
        /// <param name="lifetime">The lifetime scope for the repository.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRestOpenApi(this IServiceCollection serviceCollection, string path = "/openapi+", ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            serviceCollection.AddAttributedRestRepository<OpenApi.OpenApiRepository>();
            if (!path.EndsWith("+"))
                path += "+";
            serviceCollection.AddRestPathMapping<OpenApi.Document>(path);
            return serviceCollection;
        }
        /// <summary>
        /// Adds the OpenApiRepository endpoint to the rest services builder.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder implementation/</param>
        /// <param name="path">The endpoint path for the OpenAPI Specification document.</param>
        /// <param name="lifetime">The lifetime scope for the repository.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddOpenApi(this IRestServicesBuilder builder, string path = "/openapi+", ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            builder.ServiceCollection.AddRestOpenApi(path, lifetime);
            return builder;
        }

        /// <summary>
        /// Adds an IHttpContextManipulator to the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="manipulator">A function generating the manipulator instance.</param>
        /// <param name="lifetime">The lifetime scope.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRestContextManipulator(this IServiceCollection serviceCollection, Func<IServiceProvider, IHttpContextManipulator> manipulator, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            serviceCollection.Add(new ServiceDescriptor(typeof(IHttpContextManipulator), manipulator, lifetime));
            return serviceCollection;
        }
        /// <summary>
        /// Adds an IHttpContextManipulator to the builder's service collection.
        /// </summary>
        /// <param name="builder">A Rest services builder.</param>
        /// <param name="manipulator">A function generating the manipulator instance.</param>
        /// <param name="lifetime">The lifetime scope.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddContextManipulator(this IRestServicesBuilder builder, Func<IServiceProvider, IHttpContextManipulator> manipulator, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            builder.ServiceCollection.AddRestContextManipulator(manipulator, lifetime);
            return builder;
        }
        /// <summary>
        /// Adds the injectables for the application/problem+json media type.
        /// </summary>
        /// <param name="builder">A Rest services builder.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddProblemJson(this IRestServicesBuilder builder)
        {
            builder.AddContextManipulator(sp => ProblemContextManipulator.Json(sp.GetService<ITypeRepresentations>()));
            if (!builder.ServiceCollection.Any(sd => sd.ServiceType == typeof(ITypeRepresentation) && sd.ImplementationType == typeof(SValidationMessageProblemRepresentation)))
                builder.ServiceCollection.AddSingleton<ITypeRepresentation, SValidationMessageProblemRepresentation>();
            return builder;
        }
        /// <summary>
        /// Adds the injectables for the application/problem+xml media type.
        /// WARNING: The XmlConverter does not support the problem+xml media type yet.
        /// </summary>
        /// <param name="builder">A Rest services builder.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddProblemXml(this IRestServicesBuilder builder)
        {
            builder.AddContextManipulator(sp => ProblemContextManipulator.Xml(sp.GetService<ITypeRepresentations>()));
            if (!builder.ServiceCollection.Any(sd => sd.ServiceType == typeof(ITypeRepresentation) && sd.ImplementationType == typeof(SValidationMessageProblemRepresentation)))
                builder.ServiceCollection.AddSingleton<ITypeRepresentation, SValidationMessageProblemRepresentation>();
            return builder;
        }
        /// <summary>
        /// Adds support for custom media types instead of application/json.
        /// </summary>
        /// <param name="services">A service collection.</param>
        /// <param name="useAttributes">Indicates whether MediaTypeAttributes should be used as mappings.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRestCustomJsonMediaTypes(this IServiceCollection services, bool useAttributes = false)
        {
            services.AddRestContextManipulator(sp => CustomMediaTypeContextManipulator.Json(sp.GetService<IMediaTypeProvider>(), sp.GetService<ITypeRepresentations>()));
            if (useAttributes)
                services.AddSingleton<IMediaTypeMapping, AttributedMediaTypeMapping>();
            return services;
        }
        /// <summary>
        /// Adds support for custom media types instead of application/json.
        /// </summary>
        /// <param name="builder">A rest services builder.</param>
        /// <param name="useAttributes">Indicates whether MediaTypeAttributes should be used as mappings.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddCustomJsonMediaTypes(this IRestServicesBuilder builder, bool useAttributes = false)
        {
            builder.ServiceCollection.AddRestCustomJsonMediaTypes(useAttributes);
            return builder;
        }
        /// <summary>
        /// Adds a media type mapping for custom media types.
        /// </summary>
        /// <param name="services">A service collection.</param>
        /// <param name="mapping">The media type mapping.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRestMediaTypeMapping(this IServiceCollection services, IMediaTypeMapping mapping)
        {
            services.AddSingleton(mapping);
            return services;
        }
        /// <summary>
        /// Adds a media type mapping for custom media types.
        /// </summary>
        /// <param name="builder">A rest services builder.</param>
        /// <param name="mapping">The media type mapping.</param>
        /// <returns>The rest services builder.</returns>
        public static IRestServicesBuilder AddMediaTypeMapping(this IRestServicesBuilder builder, IMediaTypeMapping mapping)
        {
            builder.ServiceCollection.AddRestMediaTypeMapping(mapping);
            return builder;
        }
        /// <summary>
        /// Adds a media type mapping for custom media types.
        /// </summary>
        /// <typeparam name="T">The type of the media type mapping.</typeparam>
        /// <param name="builder">A rest services builder.</param>
        /// <returns>The rest services builder.</returns>
        public static IRestServicesBuilder AddMediaTypeMapping<T>(this IRestServicesBuilder builder)
            where T : class, IMediaTypeMapping
        {
            builder.ServiceCollection.AddSingleton<IMediaTypeMapping, T>();
            return builder;
        }
        
        /// <summary>
        /// Adds a media type mapping for custom media types.
        /// </summary>
        /// <param name="builder">A rest services builder.</param>
        /// <param name="type">The type the mapping applies to.</param>
        /// <param name="mediaType">The media type if the mapping applies.</param>
        /// <returns>The rest services builder.</returns>
        public static IRestServicesBuilder AddMediaTypeMapping(this IRestServicesBuilder builder, Type type, MediaType mediaType)
            => builder.AddMediaTypeMapping(new SingleMediaMapping(mediaType,type));
        /// <summary>
        /// Adds media types based on MediaTypeAttributes.
        /// </summary>
        /// <param name="builder">A rest services builder.</param>
        /// <returns>The rest services builder.</returns>
        public static IRestServicesBuilder AddAttributedMediaTypeMappings(this IRestServicesBuilder builder)
            => builder.AddMediaTypeMapping<AttributedMediaTypeMapping>();
    }
}
