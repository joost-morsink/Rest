using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Xml
{
    public static class Extensions
    {
        public static IServiceCollection AddXmlHttpConverter(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHttpRestConverter, XmlHttpConverter>();
            serviceCollection.AddSingleton(sp => new XmlSerializer(sp.GetRequiredService<TypeDescriptorCreator>(), DataConverter.Default, sp.GetServices<ITypeRepresentation>()));
            return serviceCollection;
        }
        public static IRestServicesBuilder AddXmlHttpConverter(this IRestServicesBuilder builder)
        {
            builder.ServiceCollection.AddXmlHttpConverter();
            return builder;
        }
    }
}
