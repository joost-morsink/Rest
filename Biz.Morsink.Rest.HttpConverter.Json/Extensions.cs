using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.OpenApi;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// This class contains extension methods for the configuration of a JsonHttpConverter in ASP.Net Core.
    /// </summary>
    public static class Extensions
    {
        private static Type GetGeneric(this Type type, Type interf)
            => type.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GenericTypeArguments.Length == 1 && i.GetGenericTypeDefinition() == interf)
                .Select(i => i.GenericTypeArguments[0])
                .FirstOrDefault();

        /// <summary>
        /// Adds the JsonHttpConverter to the service collection
        /// </summary>
        /// <param name="restServicesBuilder">An IRestServicesBuilder</param>
        /// <param name="builder">A function for building the JsonHttpConverter.</param>
        /// <returns>The IRestServicesBuilder.</returns>
        public static IRestServicesBuilder AddJsonHttpConverter(this IRestServicesBuilder restServicesBuilder, Func<IJsonHttpConverterBuilder, IJsonHttpConverterBuilder> builder = null)
        {
            restServicesBuilder.ServiceCollection.AddJsonHttpConverter(builder);
            return restServicesBuilder;
        }
        /// <summary>
        /// Adds the JsonHttpConverter to the service collection
        /// </summary>
        /// <param name="serviceCollection">TYhe service collection.</param>
        /// <param name="builder">A function for building the JsonHttpConverter.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddJsonHttpConverter(this IServiceCollection serviceCollection, Func<IJsonHttpConverterBuilder, IJsonHttpConverterBuilder> builder = null)
        {
            serviceCollection.AddSingleton<IHttpRestConverter, JsonHttpConverter>();

            serviceCollection.AddSingleton<JsonRestSerializer>();

            serviceCollection.AddTransient<ITypeRepresentation, OrReferenceRepresentation>();

            builder?.Invoke(new RestJsonHttpConverterBuilder(serviceCollection));
            if (!serviceCollection.Any(sd => sd.ServiceType == typeof(IOptions<JsonHttpConverterOptions>)))
                serviceCollection.AddSingleton(sp =>
                    new JsonHttpConverterOptionsProvider(opts => opts).GetOptions());
            return serviceCollection;
        }
        private class RestJsonHttpConverterBuilder : IJsonHttpConverterBuilder
        {
            public RestJsonHttpConverterBuilder(IServiceCollection serviceCollection)
            {
                ServiceCollection = serviceCollection;
            }
            public IServiceCollection ServiceCollection { get; }
        }
        /// <summary>
        /// Sets the IContractResolver to use in the JsonHttpConverter module.
        /// </summary>
        /// <typeparam name="T">The concrete type of the IContractResolver.</typeparam>
        /// <param name="builder">A builder for the JsonHttpConverter module.</param>
        /// <returns>The builder.</returns>
        public static IJsonHttpConverterBuilder AddContractResolver<T>(this IJsonHttpConverterBuilder builder)
            where T : class, IContractResolver
        {
            builder.ServiceCollection.AddSingleton<IContractResolver, T>();
            return builder;
        }
        /// <summary>
        /// Configures the JsonHttpConverterOptions.
        /// </summary>
        /// <param name="builder">A builder for the JsonHttpConverter module.</param>
        /// <param name="configure">A configure delegate.</param>
        /// <returns>The builder.</returns>
        public static IJsonHttpConverterBuilder Configure(this IJsonHttpConverterBuilder builder, Func<JsonHttpConverterOptions, JsonHttpConverterOptions> configure)
        {
            builder.ServiceCollection.AddSingleton(sp =>
                new JsonHttpConverterOptionsProvider(configure ?? (opts => opts)).GetOptions());
            return builder;
        }

    }
}
