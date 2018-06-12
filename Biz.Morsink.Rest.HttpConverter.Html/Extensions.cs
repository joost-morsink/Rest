using Biz.Morsink.Rest.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    /// <summary>
    /// Utility class for extension methods relating to the Html Http converter.
    /// </summary>
    public static class Extensions
    {
        private static Type GetGeneric(this Type type, Type interf)
            => type.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GetGenericArguments().Length == 1 && i.GetGenericTypeDefinition() == interf)
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();
        /// <summary>
        /// Adds a default HtmlHttpConverter and supporting classes to a service collection.
        /// </summary>
        /// <param name="services">A service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHtmlHttpConverter(this IServiceCollection services)
        {
            services.AddSingleton<IHttpRestConverter, HtmlHttpConverter>();
            services.AddScoped<ISpecificHtmlGeneratorProvider, SpecificHtmlGeneratorProvider>();
            services.AddSingleton<IGeneralHtmlGenerator, DefaultHtmlGenerator>();
            return services;
        }
        /// <summary>
        /// Adds the HtmlHttpConverter and supporting classes to a Rest services builder.
        /// </summary>
        /// <param name="restServicesBuilder">A builder.</param>
        /// <param name="builder">A function to build the HtmlHttpConverter.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddHtmlHttpConverter(this IRestServicesBuilder restServicesBuilder, Func<IHtmlHttpConverterBuilder, IHtmlHttpConverterBuilder> builder = null)
        {
            restServicesBuilder.ServiceCollection.AddSingleton<IHttpRestConverter, HtmlHttpConverter>();
            builder?.Invoke(new HtmlConverterBuilder(restServicesBuilder.ServiceCollection));
            restServicesBuilder.OnEndConfiguration(services =>
            {
                if (!services.Any(sd => sd.ServiceType == typeof(ISpecificHtmlGeneratorProvider)))
                    services.AddScoped<ISpecificHtmlGeneratorProvider, SpecificHtmlGeneratorProvider>();
                if (!services.Any(sd => sd.ServiceType == typeof(IGeneralHtmlGenerator)))
                    services.AddSingleton<IGeneralHtmlGenerator, DefaultHtmlGenerator>();
            });
            return restServicesBuilder;
        }
        /// <summary>
        /// Adds a general Html generator to a service collection.
        /// </summary>
        /// <typeparam name="T">The type of a general Html generator.</typeparam>
        /// <param name="serviceDescriptors">A service collection.</param>
        /// <param name="scope">The scope for the generator.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddGeneralHtmlGenerator<T>(this IServiceCollection serviceDescriptors, ServiceLifetime scope = ServiceLifetime.Scoped)
            where T : IGeneralHtmlGenerator
        {
            serviceDescriptors.Add(new ServiceDescriptor(typeof(IGeneralHtmlGenerator), typeof(T), scope));
            return serviceDescriptors;
        }
        /// <summary>
        /// Adds a general Html generator to an Html converter builder.
        /// </summary>
        /// <typeparam name="T">The type of a general Html generator.</typeparam>
        /// <param name="builder">An Html Http converter builder.</param>
        /// <param name="scope">The scope for the generator.</param>
        /// <returns>The builder.</returns>
        public static IHtmlHttpConverterBuilder AddGeneralGenerator<T>(this IHtmlHttpConverterBuilder builder, ServiceLifetime scope = ServiceLifetime.Scoped)
            where T : IGeneralHtmlGenerator
        {
            builder.ServiceCollection.AddGeneralHtmlGenerator<T>(scope);
            return builder;
        }
        /// <summary>
        /// Adds a specific Html generator to a service collection.
        /// </summary>
        /// <typeparam name="T">The type of a specific Html generator.</typeparam>
        /// <param name="serviceDescriptors">A service collection.</param>
        /// <param name="scope">The scope for the generator.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddSpecificHtmlGenerator<T>(this IServiceCollection serviceDescriptors, ServiceLifetime scope = ServiceLifetime.Scoped)
            where T : ISpecificHtmlGenerator
        {
            var type = typeof(T).GetGeneric(typeof(ISpecificHtmlGenerator<>));
            serviceDescriptors.Add(new ServiceDescriptor(typeof(ISpecificHtmlGenerator), typeof(T), scope));
            serviceDescriptors.Add(new ServiceDescriptor(typeof(ISpecificHtmlGenerator<>).MakeGenericType(type), typeof(T), scope));
            return serviceDescriptors;
        }
        /// <summary>
        /// Adds a specific Html generator to an Html converter builder.
        /// </summary>
        /// <typeparam name="T">The type of a specific Html generator.</typeparam>
        /// <param name="builder">An Html Http converter builder.</param>
        /// <param name="scope">The scope for the generator.</param>
        /// <returns>The builder.</returns>
        public static IHtmlHttpConverterBuilder AddGenerator<T>(IHtmlHttpConverterBuilder builder, ServiceLifetime scope = ServiceLifetime.Scoped)
            where T : ISpecificHtmlGenerator
        {
            builder.ServiceCollection.AddSpecificHtmlGenerator<T>(scope);
            return builder;
        }
        private class HtmlConverterBuilder : IHtmlHttpConverterBuilder
        {
            public HtmlConverterBuilder(IServiceCollection services)
            {
                ServiceCollection = services;
            }

            public IServiceCollection ServiceCollection { get; }
        }
    }
}
