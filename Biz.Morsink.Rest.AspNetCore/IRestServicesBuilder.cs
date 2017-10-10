using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// A decorating interface over IServiceCollection to apply Rest related configuration.
    /// </summary>
    public interface IRestServicesBuilder
    {
        /// <summary>
        /// The underlying IServiceCollection.
        /// </summary>
        IServiceCollection ServiceCollection { get; }
    }
}
