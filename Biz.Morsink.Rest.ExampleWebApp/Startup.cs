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
using Biz.Morsink.Rest.HttpConverter.Xml;
using Newtonsoft.Json;
using Biz.Morsink.Rest.AspNetCore.Caching;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Options;
using Biz.Morsink.Rest.AspNetCore.Identity;
using System.Security.Claims;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRest(bld => bld
                // Configure the basics
                .AddDefaultServices()
                .AddDefaultIdentityProvider("http://localhost:5000", new RestPrefix("http://localhost:5000", "api"))
                .AddCache<RestMemoryCache>()
                .AddJobs()
                .UseRequestHandler((sp, hbld) => hbld.Use<CancelRequestHandler>(sp, TimeSpan.FromSeconds(30.0)))
                // Configure HttpConverters
                .AddJsonHttpConverter(jbld => jbld.Configure(opts => opts.ApplyCamelCaseNamingStrategy()))
                .AddXmlHttpConverter()
                // Configure Repositories
                .AddStructure<PersonStructure.Structure>(ServiceLifetime.Singleton)
                // or: .AddCollection<PersonCollectionRepository, PersonRepository, PersonSource>(sourceLifetime: ServiceLifetime.Singleton)
                .AddStructure<BlogRepository.Structure>()
                .AddStructure<ExceptionRepository.Structure>()
                .AddStructure<OpenApiRepository.Structure>()
                .AddRepository<HomeRepository>()
                );
            services.Configure<RestAspNetCoreOptions>(opts => { opts.UseCuries = false; });
            services.AddTransient<ITokenProvider<Person>, HashTokenProvider<Person>>();
            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.Use(next => context =>
            {
#if USERTEST
                context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Joost") }));
#endif
                return next(context);
            });
            app.UseRest();
            app.Run(async context =>
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Unreachable.");
            });
        }
    }
}
