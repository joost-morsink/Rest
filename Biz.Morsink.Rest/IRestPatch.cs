using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface that defines the PATCH capability.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <typeparam name="P">The parameter type.</typeparam>
    /// <typeparam name="I">The instructions type.</typeparam>
    [Capability("PATCH")]
    public interface IRestPatch<T, P, I>
        where T : class
    {
        /// <summary>
        /// Patches a resource.
        /// </summary>
        /// <param name="address">The identity value of the target resource.</param>
        /// <param name="parameters">The parameters for the PATCH operation.</param>
        /// <param name="patchInstructions">The patch instructions to apply to the reource.</param>
        /// <returns>The updated resource.</returns>
        ValueTask<RestResponse<T>> Patch(IIdentity<T> address, P parameters, I patchInstructions, CancellationToken cancellationToken);
    }
}
