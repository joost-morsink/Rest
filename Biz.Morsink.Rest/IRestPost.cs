using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface that defines the POST capability.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <typeparam name="P">The parameters type.</typeparam>
    /// <typeparam name="E">The entity type.</typeparam>
    /// <typeparam name="R">The return type.</typeparam>
    [Capability("POST")]
    public interface IRestPost<T, P, E, R> : IRestCapability<T>
    {
        /// <summary>
        /// Posts a document to some resource.
        /// </summary>
        /// <param name="target">The identity value of the target resource.</param>
        /// <param name="parameters">The parameters for the POST operation.</param>
        /// <param name="entity">The entity to post to the resource.</param>
        /// <returns>A response potentially containing a typed result.</returns>
        ValueTask<RestResponse<R>> Post(IIdentity<T> target, P parameters, E entity, CancellationToken cancellationToken);
    }
}
