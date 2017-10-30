using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        /// Value for the not authenticated HTTP status.
        /// </summary>
        public const int STATUS_NOTAUTHENTICATED = 401;
        /// <summary>
        /// Value for the forbidden HTTP status.
        /// </summary>
        public const int STATUS_FORBIDDEN = 403;
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
        private readonly IAuthorizationProvider authorizationProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next request delegate in the pipeline.</param>
        /// <param name="restHandler">A Rest request handler.</param>
        /// <param name="httpHandler">A Rest HTTP pipeline.</param>
        /// <param name="identityProvider">A Rest identity provider.</param>
        /// <param name="converters">A collection of applicable Rest converters for HTTP.</param>
        public RestForAspNetCore(RequestDelegate next, IRestRequestHandler restHandler, IHttpRestRequestHandler httpHandler, IRestIdentityProvider identityProvider, IEnumerable<IHttpRestConverter> converters, IAuthorizationProvider authorizationProvider)
        {
            this.handler = restHandler;
            this.converters = converters.ToArray();
            this.identityProvider = identityProvider;
            this.restRequestDelegate = httpHandler.GetRequestDelegate(restHandler);
            this.authorizationProvider = authorizationProvider;
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
                    if(authorizationProvider.IsAllowed(context.User, req.Address, req.Capability))
                    {
                        var resp = await restRequestDelegate(context, req, conv);
                        await WriteResponse(conv, context, resp);
                    }
                    else
                    {
                        // TODO: The following status assignment is very simplistic and should be refactored at a later stage.
                        context.Response.StatusCode = context.User.Identity.IsAuthenticated ? STATUS_FORBIDDEN : STATUS_NOTAUTHENTICATED;
                    }
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
        public static IApplicationBuilder UseRest(this IApplicationBuilder app)
        {
            {   // Prime the schema cache:
                var repositories = app.ApplicationServices.GetServices<IRestRepository>();
                var typeDescriptorCreator = app.ApplicationServices.GetRequiredService<TypeDescriptorCreator>();
                foreach (var type in repositories.SelectMany(repo => repo.SchemaTypes).Distinct())
                    typeDescriptorCreator.GetDescriptor(type);
                typeDescriptorCreator.GetDescriptor(typeof(TypeDescriptor));
                typeDescriptorCreator.GetDescriptor(typeof(RestCapabilities));

                // Prime attribute based rest identity provider:
                var idProv = app.ApplicationServices.GetService<IRestIdentityProvider>() as AttributeBasedRestIdentityProvider;
                idProv?.Initialize(repositories);
            }

            return app.UseMiddleware<RestForAspNetCore>();
        }
        /// <summary>
        /// Adds services for RestForAspNetCore to the specified service collection.
        /// RestForAspNetCore depends on the IHttpContextAccessor implementation for its security implementation,
        /// and registers the HttpContextAccessor class as implementation.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add RestForAspNetCore to.</param>
        /// <param name="builder">A Rest Services Builder.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRest(this IServiceCollection serviceCollection, Action<IRestServicesBuilder> builder = null)
        {
            serviceCollection.AddSingleton<CoreRestRequestHandler>();
            serviceCollection.AddSingleton<TypeDescriptorCreator>();
            serviceCollection.AddRestRepository<SchemaRepository>();
            serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            serviceCollection.AddTransient<IUser, AspNetCoreUser>();

            var restbuilder = new RestServicesBuilder(serviceCollection);
            builder?.Invoke(restbuilder);
            restbuilder.EndConfiguration();

            if (!serviceCollection.Any(sd => sd.ServiceType == typeof(IAuthorizationProvider)))
                serviceCollection.AddScoped<IAuthorizationProvider, AlwaysAllowAuthorizationProvider>();
            if (!serviceCollection.Any(sd => sd.ServiceType == typeof(IRestIdentityProvider)))
                throw new InvalidOperationException("Rest component depends on an IRestIdentityProvider implementation.");
            return serviceCollection;
        }

        private class RestServicesBuilder : IRestServicesBuilder
        {
            public Func<IServiceProvider, IHttpRestRequestHandler, IHttpRestRequestHandler> httpHandlerConfigurator;
            public Func<IServiceProvider, IRestRequestHandlerBuilder, IRestRequestHandlerBuilder> restHandlerConfigurator;
            public RestServicesBuilder(IServiceCollection serviceCollection)
            {
                ServiceCollection = serviceCollection;
                httpHandlerConfigurator = (sp, x) => x;
                restHandlerConfigurator = (sp, x) => x;
            }
            public IServiceCollection ServiceCollection { get; }

            public void EndConfiguration()
            {
                ServiceCollection.AddSingleton(sp => httpHandlerConfigurator(sp, HttpRestRequestHandler.Create()));
                ServiceCollection.AddSingleton(sp => restHandlerConfigurator(sp, RestRequestHandlerBuilder.Create())
                    .Run(() => sp.GetRequiredService<CoreRestRequestHandler>().HandleRequest));
            }
            private Func<T, T> Compose<T>(Func<T, T> f, Func<T, T> g) => x => f(g(x));
            private Func<X, T, T> Compose<X, T>(Func<X, T, T> f, Func<X, T, T> g) => (x, y) => f(x, g(x, y));

            public IRestServicesBuilder UseHttpRequestHandler(Func<IServiceProvider, IHttpRestRequestHandler, IHttpRestRequestHandler> configurator)
            {
                httpHandlerConfigurator = Compose(configurator, httpHandlerConfigurator);
                return this;
            }

            public IRestServicesBuilder UseRequestHandler(Func<IServiceProvider, IRestRequestHandlerBuilder, IRestRequestHandlerBuilder> configurator)
            {
                restHandlerConfigurator = Compose(configurator, restHandlerConfigurator);
                return this;
            }
        }
    }
}
