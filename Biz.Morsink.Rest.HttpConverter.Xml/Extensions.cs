using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public static class Extensions
    {
        /// <summary>
        /// Adds an IJsonSchemaTranslator to the service collection as all implemented interfaces relevant to Json schema translation.
        /// </summary>
        /// <typeparam name="T">A Json Schema Translator type.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>The service collection with added registrations.</returns>
        public static IServiceCollection AddXmlSchemaTranslator<T>(this IServiceCollection serviceCollection)
            where T : IXmlSchemaTranslator
        {
            var gen = typeof(T).GetGeneric(typeof(IXmlSchemaTranslator<>));
            serviceCollection.Add(new ServiceDescriptor(typeof(IXmlSchemaTranslator), typeof(T), ServiceLifetime.Singleton));
            serviceCollection.Add(new ServiceDescriptor(typeof(ISchemaTranslator<XmlSchema>), typeof(T), ServiceLifetime.Singleton));
            serviceCollection.Add(new ServiceDescriptor(typeof(IXmlSchemaTranslator<>).MakeGenericType(gen), typeof(T), ServiceLifetime.Singleton));
            serviceCollection.Add(new ServiceDescriptor(typeof(ISchemaTranslator<,>).MakeGenericType(gen, typeof(XmlSchema)), typeof(T), ServiceLifetime.Singleton));
            return serviceCollection;
        }

        /// <summary>
        /// Adds the Xml Http Converter to the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>The service collection with the added registrations.</returns>
        public static IServiceCollection AddXmlHttpConverter(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHttpRestConverter, XmlHttpConverter>();
            serviceCollection.AddSingleton(sp => new XmlSerializer(sp.GetRequiredService<TypeDescriptorCreator>(), DataConverter.Default, sp.GetServices<IXmlSchemaTranslator>(), sp.GetServices<ITypeRepresentation>()));
            serviceCollection.AddXmlSchemaTranslator<XmlSchemaXmlSchemaTranslator>();
            serviceCollection.AddXmlSchemaTranslator<OpenApiXmlSchemaTranslator>();
            return serviceCollection;
        }
        /// <summary>
        /// Adds the Xml Http Converter to the service collection.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder instance.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder AddXmlHttpConverter(this IRestServicesBuilder builder)
        {
            builder.ServiceCollection.AddXmlHttpConverter();
            return builder;
        }
    }
}
