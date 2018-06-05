using Biz.Morsink.Rest.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Html
{
    public static class Extensions
    {
        public static IServiceCollection AddHtmlHttpConverter(this IServiceCollection services)
        {
            services.AddSingleton<IHttpRestConverter, HtmlHttpConverter>();
            services.AddSingleton<IGeneralHtmlGenerator, TestHtmlGenerator>();
            return services;
        }
        public static IRestServicesBuilder AddHtmlHttpConverter(this IRestServicesBuilder builder)
        {
            builder.ServiceCollection.AddHtmlHttpConverter();
            return builder;
        }
    }
}
