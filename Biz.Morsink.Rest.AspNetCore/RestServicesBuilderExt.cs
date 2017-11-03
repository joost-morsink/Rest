using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Helper extension methods for IRestServicesBuilder
    /// </summary>
    public static class RestServicesBuilderExt
    {
        /// <summary>
        /// Adds components to the Rest HTTP pipeline.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder.</param>
        /// <param name="configurator">A configurator function for the Rest HTTP pipeline</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder UseHttpRequestHandler(this IRestServicesBuilder builder, Func<IHttpRestRequestHandler, IHttpRestRequestHandler> configurator)
            => builder.UseHttpRequestHandler((sp, x) => configurator(x));
        /// <summary>
        /// Adds components to the Rest request handler pipeline.
        /// </summary>
        /// <param name="builder">An IRestServicesBuilder.</param>
        /// <param name="configurator">A configurator function for the Rest request handler pipeline.</param>
        /// <returns>The builder.</returns>
        public static IRestServicesBuilder UseRequestHandler(this IRestServicesBuilder builder, Func<IRestRequestHandlerBuilder, IRestRequestHandlerBuilder> configurator)
            => builder.UseRequestHandler((sp, x) => configurator(x));
    }
}
