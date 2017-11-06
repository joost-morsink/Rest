using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface that defines the PUT capability.
    /// A PUT operation should be idempotent.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <typeparam name="P">The parameter type.</typeparam>
    [Capability("PUT")]
    public interface IRestPut<T, P> : IRestCapability<T>
        where T : class
    {
        /// <summary>
        /// Puts a resource.
        /// </summary>
        /// <param name="target">The identity value of the target resource.</param>
        /// <param name="parameters">THe parameters for the put operation.</param>
        /// <param name="entity">The resource to put.</param>
        /// <returns>A response potentially containing the up-to-date resource.</returns>
        ValueTask<RestResponse<T>> Put(IIdentity<T> target, P parameters, T entity, CancellationToken cancellationToken);
    }
}
