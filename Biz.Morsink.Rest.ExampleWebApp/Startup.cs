using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.HttpConverter.Json;
using Newtonsoft.Json;
using Biz.Morsink.Rest.Schema;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class Startup
    {
        private IContainer container;
        //private ILifetimeScope handlerLifetime;

        private class AutofacServiceLocator : IServiceLocator
        {
            private readonly ILifetimeScope scope;

            public AutofacServiceLocator(ILifetimeScope scope)
            {
                this.scope = scope;
            }

            public object ResolveOptional(Type t)
                => scope.ResolveOptional(t);

            public object ResolveRequired(Type t)
                => scope.Resolve(t);

            public IEnumerable<object> ResolveMulti(Type t)
                => scope.Resolve(typeof(IEnumerable<>).MakeGenericType(t)) as IEnumerable<object>;
        }

        public IRestRequestHandlerBuilder ConfigureRestRequestHandler(IRestRequestHandlerBuilder builder)
        {
            return builder;
        }
        public IRestHttpPipeline ConfigurePipeline(IRestHttpPipeline pipeline)
        {
            return pipeline;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<JsonHttpConverterOptions>(options => { });
            var cb = new ContainerBuilder();
            cb.RegisterType<CoreRestRequestHandler>().AsSelf().SingleInstance();
            cb.RegisterType<AutofacServiceLocator>().AsImplementedInterfaces();
            cb.RegisterType<PersonRepository>().As<IRestRepository>().As<IRestRepository<Person>>();
            cb.RegisterType<HomeRepository>().As<IRestRepository>().As<IRestRepository<Home>>();
            cb.RegisterType<SchemaRepository>().As<IRestRepository<TypeDescriptor>>().SingleInstance();
            cb.RegisterType<ExampleRestIdentityProvider>().AsImplementedInterfaces();
            cb.RegisterType<JsonHttpConverter>().AsImplementedInterfaces();
            cb.RegisterInstance(ConfigurePipeline(RestHttpPipeline.Create()));
            var builder = ConfigureRestRequestHandler(RestRequestHandlerBuilder.Create());
            cb.Register(cc => builder.Run(() => cc.Resolve<CoreRestRequestHandler>().HandleRequest)).SingleInstance();
            cb.Populate(services);
            container = cb.Build();
            return new AutofacServiceProvider(container);
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
            //lifetime.ApplicationStopping.Register(() => handlerLifetime.Dispose());
        }
    }
}
