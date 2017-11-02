using Biz.Morsink.Rest.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Interface specifying a structure of Rest components.
    /// </summary>
    public interface IRestStructure
    {
        /// <summary>
        /// The structure should register its components to a serviceCollection using this method.
        /// </summary>
        /// <param name="serviceCollection">The service collection the components will be registered in.</param>
        /// <param name="lifetime">The default lifetime scope of the root component.</param>
        void RegisterComponents(IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Scoped);
    }
}
