﻿using System;
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
using Biz.Morsink.Rest.AspNetCore.Caching;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRest(bld => bld
                // Configure the basics
                .AddDefaultServices()
                .AddDefaultIdentityProvider()
                .AddCache<RestMemoryCache>()
                // Configure HttpConverters
                .AddJsonHttpConverter()
                // Configure Repositories
                .AddCollection<PersonCollectionRepository, PersonRepository, PersonSource>(sourceLifetime: ServiceLifetime.Singleton)
                .AddRepository<HomeRepository>()
                );
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
            app.UseRest();
            app.Run(async context =>
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Unreachable.");
            });
        }
    }
}
