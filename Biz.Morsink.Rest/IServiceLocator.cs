using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A service locator interface for use in the Rest routing system.
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// Resolve a required service.
        /// </summary>
        /// <param name="t">The type of service to resolve.</param>
        /// <returns>
        /// An instance of the service-type. 
        /// Throws if the service cannot be resolved.
        /// </returns>
        object ResolveRequired(Type t);
        /// <summary>
        /// Resolve an optional service.
        /// </summary>
        /// <param name="t">The type of service to resolve.</param>
        /// <returns>An instance of the service type, or null if it cannot be resolved.</returns>
        object ResolveOptional(Type t);
        /// <summary>
        /// Resolves all instances of services of a certain type.
        /// </summary>
        /// <param name="t">The type of service to resolve.</param>
        /// <returns>A collection of services that implement the required service type.</returns>
        IEnumerable<object> ResolveMulti(Type t);
    }
}
