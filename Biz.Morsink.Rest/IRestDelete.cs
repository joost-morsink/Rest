using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface that defines the DELETE capability.
    /// A DELETE should be idempotent, but may return different results on subsequent calls. (like NotFound)
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <typeparam name="P">The parameter type.</typeparam>
    [Capability("DELETE")]
    public interface IRestDelete<T, P> : IRestCapability<T>
    {
        /// <summary>
        /// Deletes a resource.
        /// </summary>
        /// <param name="target">The identity value of the resource to delete.</param>
        /// <param name="parameters">A parameters object for deletion.</param>
        /// <returns>A response for the deletion.</returns>
        ValueTask<RestResponse<object>> Delete(IIdentity<T> target, P parameters, CancellationToken cancellationToken);
    }
}
