using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Jobs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.Utils
{
    /// <summary>
    /// Extension methods for Rest classes.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Determines if a Link is allowed to be opened by a principal according to an authorization provider.
        /// </summary>
        /// <param name="link">The Link to check.</param>
        /// <param name="provider">The authorization provider that will determine if access is allowed.</param>
        /// <param name="principal">The principal requesting access to the link.</param>
        /// <returns>True if access is allowed.</returns>
        public static bool IsAllowedBy(this Link link, IAuthorizationProvider provider, ClaimsPrincipal principal)
            => provider == null || provider.IsAllowed(principal, link.Target, getCapabilityString(link.Capability));

        /// <summary>
        /// Determines if a Link is allowed to be opened by a principal according to an authorization provider.
        /// </summary>
        /// <param name="link">The Link to check.</param>
        /// <param name="provider">The authorization provider that will determine if access is allowed.</param>
        /// <param name="user">The user requesting access to the link.</param>
        /// <returns>True if access is allowed.</returns>        
        public static bool IsAllowedBy(this Link link, IAuthorizationProvider provider, IUser user)
            => provider == null || provider.IsAllowed(user?.Principal, link.Target, getCapabilityString(link.Capability));

        public static string GetCapabilityString(this Link link)
            => link.Capability.GetTypeInfo().GetCustomAttribute<CapabilityAttribute>().Name;

        private static string getCapabilityString(Type capability)
            => capability.GetTypeInfo().GetCustomAttribute<CapabilityAttribute>().Name;

        internal static IEnumerable<T> Iterate<T>(this T seed, Func<T,T> next)
        {
            while (true)
            {
                yield return seed;
                seed = next(seed);
            }
        }
        /// <summary>
        /// Gets a job controller from the job store.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="id">The identity value for the job controller.</param>
        /// <returns>A RestJobController instance if it was found, null otherwise.</returns>
        public static ValueTask<RestJobController> GetController(this IRestJobStore store, IIdentity<RestJobController> id)
            => store.GetController(id as IIdentity<RestJob, RestJobController>);
    }
}
