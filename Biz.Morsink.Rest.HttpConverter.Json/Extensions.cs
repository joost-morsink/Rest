using Biz.Morsink.Rest.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
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
                .Where(i => i.GetGenericArguments().Length == 1 && i.GetGenericTypeDefinition() == interf)
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();
        /// <summary>
        /// Adds an IJsonSchemaTranslator to the service collection as all implemented interfaces relevant to Json schema translation.
        /// </summary>
        /// <typeparam name="T">A Json Schema Translator type.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>The service collection with added registrations.</returns>
        public static IServiceCollection AddJsonSchemaTranslator<T>(this IServiceCollection serviceCollection)
            where T : IJsonSchemaTranslator
        {
            var gen = typeof(T).GetGeneric(typeof(IJsonSchemaTranslator<>));
            serviceCollection.Add(new ServiceDescriptor(typeof(IJsonSchemaTranslator), typeof(T), ServiceLifetime.Singleton));
            serviceCollection.Add(new ServiceDescriptor(typeof(ISchemaTranslator<JsonSchema>), typeof(T), ServiceLifetime.Singleton));
            serviceCollection.Add(new ServiceDescriptor(typeof(IJsonSchemaTranslator<>).MakeGenericType(gen), typeof(T), ServiceLifetime.Singleton));
            serviceCollection.Add(new ServiceDescriptor(typeof(ISchemaTranslator<,>).MakeGenericType(gen, typeof(JsonSchema)), typeof(T), ServiceLifetime.Singleton));
            return serviceCollection;
        }
    }
}
