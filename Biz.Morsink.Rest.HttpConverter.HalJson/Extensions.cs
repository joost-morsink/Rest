using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public static class Extensions
    {
        /// <summary>
        /// Adds the HalJsonHttpConverter to the service collection
        /// </summary>
        /// <param name="restServicesBuilder">An IRestServicesBuilder</param>
        /// <param name="builder">A function for building the HalJsonHttpConverter.</param>
        /// <returns>The IRestServicesBuilder.</returns>
        public static IRestServicesBuilder AddHalJsonHttpConverter(this IRestServicesBuilder restServicesBuilder, Func<IHalJsonHttpConverterBuilder, IHalJsonHttpConverterBuilder> builder = null)
        {
            restServicesBuilder.ServiceCollection.AddHalJsonHttpConverter(builder);
            return restServicesBuilder;
        }
        /// <summary>
        /// Adds the JsonHttpConverter to the service collection
        /// </summary>
        /// <param name="serviceCollection">TYhe service collection.</param>
        /// <param name="builder">A function for building the JsonHttpConverter.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHalJsonHttpConverter(this IServiceCollection serviceCollection, Func<IHalJsonHttpConverterBuilder, IHalJsonHttpConverterBuilder> builder = null)
        {
            serviceCollection.AddSingleton<IHttpRestConverter, HalJsonHttpConverter>();
            serviceCollection.AddSingleton(sp => new HalSerializer(
                sp.GetRequiredService<TypeDescriptorCreator>(),
                DataConverter.Default,
                sp.GetServices<ITypeRepresentation>()));
            //serviceCollection.AddJsonSchemaTranslator<TypeDescriptorConverter>();

            //serviceCollection.AddJsonSchemaTranslator<OrReferenceConverter<Parameter>>();
            //serviceCollection.AddJsonSchemaTranslator<OrReferenceConverter<Header>>();
            //serviceCollection.AddJsonSchemaTranslator<OrReferenceConverter<AspNetCore.OpenApi.Schema>>();

            //serviceCollection.AddSingleton<IJsonSchemaProvider, JsonSchemaProvider>();

            builder?.Invoke(new RestHalJsonHttpConverterBuilder(serviceCollection));
            //if (!serviceCollection.Any(sd => sd.ServiceType == typeof(IContractResolver)))
            //    serviceCollection.AddSingleton<IContractResolver, RestJsonContractResolver>();
            return serviceCollection;
        }
        private class RestHalJsonHttpConverterBuilder : IHalJsonHttpConverterBuilder
        {
            public RestHalJsonHttpConverterBuilder(IServiceCollection serviceCollection)
            {
                ServiceCollection = serviceCollection;
            }
            public IServiceCollection ServiceCollection { get; }
        }
    }
}
