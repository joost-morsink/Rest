using Biz.Morsink.Rest.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// An ASP.Net Core middleware component that wraps all the necessary components for handling Restful requests.
    /// </summary>
    public class RestForAspNetCore
    {
        /// <summary>
        /// Value for the not found HTTP status.
        /// </summary>
        public const int STATUS_NOTFOUND = 404;
        /// <summary>
        /// Value for the internal server error HTTP status.
        /// </summary>
        public const int STATUS_INTERNALSERVERERROR = 500;

        private readonly IRestRequestHandler handler;
        private readonly IHttpRestConverter[] converters;
        private readonly IRestIdentityProvider identityProvider;
        private readonly RestRequestDelegate restRequestDelegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next request delegate in the pipeline.</param>
        /// <param name="handler">A Rest request handler.</param>
        /// <param name="pipeline">A Rest HTTP pipeline.</param>
        /// <param name="identityProvider">A Rest identity provider.</param>
        /// <param name="converters">A collection of applicable Rest converters for HTTP.</param>
        public RestForAspNetCore(RequestDelegate next, IRestRequestHandler handler, IRestHttpPipeline pipeline, IRestIdentityProvider identityProvider, IEnumerable<IHttpRestConverter> converters)
        {
            this.handler = handler;
            this.converters = converters.ToArray();
            this.identityProvider = identityProvider;
            this.restRequestDelegate = pipeline.GetRequestDelegate(handler);
        }
        /// <summary>
        /// This method implements the RequestDelegate for the Rest middleware component.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                var (req, conv) = ReadRequest(context);
                if (req == null)
                {
                    context.Response.StatusCode = STATUS_NOTFOUND;
                    await context.Response.WriteAsync("Cannot find resource");
                }
                else
                {
                    var resp = await restRequestDelegate(context, req, conv);
                    await WriteResponse(conv, context, resp);
                }
            }
            catch
            {
                context.Response.StatusCode = STATUS_INTERNALSERVERERROR;
                await context.Response.WriteAsync("An error occured.");
            }
        }
        private (RestRequest, IHttpRestConverter) ReadRequest(HttpContext context)
        {
            var request = context.Request;
            var req = RestRequest.Create(request.Method, identityProvider.Parse(request.Path + request.QueryString),
                request.Query.SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<string, string>(kvp.Key, v))));
            for (int i = 0; i < converters.Length; i++)
                if (converters[i].Applies(context))
                    return (converters[i].ManipulateRequest(req, context), converters[i]);
            return (null, null);
        }
        private Task WriteResponse(IHttpRestConverter converter, HttpContext context, RestResponse response)
        {
            return converter.SerializeResponse(response, context);
        }
    }
    /// <summary>
    /// Helper class for extension methods.
    /// </summary>
    public static class RestForAspNetCoreExt
    {
        /// <summary>
        /// Adds an RestForAspNetCore middleware component to an application pipeline.
        /// </summary>
        /// <param name="app">The application builder/</param>
        /// <returns>An application builder.</returns>
        public static IApplicationBuilder UseRestForAspNetCore(this IApplicationBuilder app)
        {
            {   // Prime the schema cache:
                var repositories = app.ApplicationServices.GetServices<IRestRepository>();
                var typeDescriptorCreator = app.ApplicationServices.GetRequiredService<TypeDescriptorCreator>();
                foreach (var type in repositories.SelectMany(repo => repo.SchemaTypes).Distinct())
                    typeDescriptorCreator.GetDescriptor(type);
                typeDescriptorCreator.GetDescriptor(typeof(TypeDescriptor));
            }

            return app.UseMiddleware<RestForAspNetCore>();
        }
        /// <summary>
        /// Adds services for RestForAspNetCore to the specified service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add RestForAspNetCore to.</param>
        /// <param name="builder">A Rest Services Builder.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRestForAspNetCore(this IServiceCollection serviceCollection, Action<IRestServicesBuilder> builder = null)
        {
            serviceCollection.AddSingleton<CoreRestRequestHandler>();
            serviceCollection.AddSingleton<TypeDescriptorCreator>();
            serviceCollection.AddRestRepository<SchemaRepository>();

            var restbuilder = new RestServicesBuilder(serviceCollection);
            builder?.Invoke(restbuilder);
            restbuilder.EndConfiguration();

            if (!serviceCollection.Any(sd => sd.ServiceType == typeof(IRestIdentityProvider)))
                throw new InvalidOperationException("Rest component depends on an IRestIdentityProvider implementation.");
            return serviceCollection;
        }

        private class RestServicesBuilder : IRestServicesBuilder
        {
            public Func<IServiceProvider, IRestHttpPipeline, IRestHttpPipeline> pipelineConfigurator;
            public Func<IServiceProvider, IRestRequestHandlerBuilder, IRestRequestHandlerBuilder> requestHandlerConfigurator;
            public RestServicesBuilder(IServiceCollection serviceCollection)
            {
                ServiceCollection = serviceCollection;
                pipelineConfigurator = (sp, x) => x;
                requestHandlerConfigurator = (sp, x) => x;
            }
            public IServiceCollection ServiceCollection { get; }

            public void EndConfiguration()
            {
                ServiceCollection.AddSingleton(sp => pipelineConfigurator(sp, RestHttpPipeline.Create()));
                ServiceCollection.AddSingleton(sp => requestHandlerConfigurator(sp, RestRequestHandlerBuilder.Create())
                    .Run(() => sp.GetRequiredService<CoreRestRequestHandler>().HandleRequest));
            }
            private Func<T, T> Compose<T>(Func<T, T> f, Func<T, T> g) => x => f(g(x));
            private Func<X, T, T> Compose<X, T>(Func<X, T, T> f, Func<X, T, T> g) => (x, y) => f(x, g(x, y));

            public IRestServicesBuilder UsePipeline(Func<IServiceProvider, IRestHttpPipeline, IRestHttpPipeline> configurator)
            {
                pipelineConfigurator = Compose(configurator, pipelineConfigurator);
                return this;
            }

            public IRestServicesBuilder UseRequestHandler(Func<IServiceProvider, IRestRequestHandlerBuilder, IRestRequestHandlerBuilder> configurator)
            {
                requestHandlerConfigurator = Compose(configurator, requestHandlerConfigurator);
                return this;
            }
        }
    }
}
