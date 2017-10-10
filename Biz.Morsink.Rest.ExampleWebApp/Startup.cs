using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.HttpConverter.Json;
using Newtonsoft.Json;
using Biz.Morsink.Rest.Schema;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class Startup
    {
        public IRestRequestHandlerBuilder ConfigureRestRequestHandler(IRestRequestHandlerBuilder builder)
        {
            return builder;
        }
        public IRestHttpPipeline ConfigurePipeline(IRestHttpPipeline pipeline)
        {
            return pipeline.UseCapabilityDiscovery();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IOptions<JsonHttpConverterOptions>>(sp => new JsonHttpConverterOptionsProvider(sp.GetRequiredService<IContractResolver>(), opts => opts));
            services.AddSingleton<CoreRestRequestHandler>();

            services.AddRestRepository<PersonRepository>()
                .AddRestRepository<HomeRepository>()
                .AddRestRepository<SchemaRepository>(ServiceLifetime.Singleton);

            services.AddScoped<IRestIdentityProvider, ExampleRestIdentityProvider>();
            services.AddSingleton<IContractResolver, RestJsonContractResolver>();
            services.AddSingleton<IHttpRestConverter, JsonHttpConverter>();
            services.AddJsonSchemaTranslator<IdentityConverter>()
                .AddJsonSchemaTranslator<TypeDescriptorConverter>();

            services.AddSingleton(ConfigurePipeline(RestHttpPipeline.Create()));
            services.AddSingleton(sp => ConfigureRestRequestHandler(RestRequestHandlerBuilder.Create())
                .Run(() => sp.GetRequiredService<CoreRestRequestHandler>().HandleRequest));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRestForAspNetCore();
            app.Run(async context =>
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Unreachable.");
            });
        }
    }
}
