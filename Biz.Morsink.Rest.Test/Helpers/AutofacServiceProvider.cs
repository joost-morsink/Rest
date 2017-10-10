using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Test.Helpers
{
    /// <summary>
    /// An implementation of the IServiceProvider interface for Autofac.
    /// </summary>
    class AutofacServiceProvider : IServiceProvider
    {
        private readonly ILifetimeScope scope;

        /// <summary>
        /// Constructor≥
        /// </summary>
        /// <param name="scope">The ILifetimeScope instance that should be used by the IServiceProvider.</param>
        public AutofacServiceProvider(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        /// <summary>
        /// Tries to resolve a service.
        /// </summary>
        /// <param name="serviceType">The type of service.</param>
        /// <returns>A service instance if one of the specified type can be found. Otherwise, null.</returns>
        public object GetService(Type serviceType)
            => scope.ResolveOptional(serviceType);
    }
}
