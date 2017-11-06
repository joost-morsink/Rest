using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using System.Threading;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface that defines the GET capability.
    /// A GET should be both safe and idempotent.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <typeparam name="P">The parameter type.</typeparam>
    [Capability("GET")]
    public interface IRestGet<T, P> : IRestCapability<T>
        where T : class
    {
        /// <summary>
        /// Gets a resource.
        /// </summary>
        /// <param name="id">The identity value of the resource.</param>
        /// <param name="parameters">Parameters used to get the resource.</param>
        /// <returns>A response potentially containing a resource.</returns>
        ValueTask<RestResponse<T>> Get(IIdentity<T> id, P parameters, CancellationToken cancellationToken);
    }

}
