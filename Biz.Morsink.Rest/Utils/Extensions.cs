using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Jobs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        internal static IEnumerable<T> Iterate<T>(this T seed, Func<T, T> next)
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

        public static string GetRestDocumentation(this IEnumerable<RestDocumentationAttribute> attributes)
        {
            var docs = from a in attributes
                       where a.Format == "text/plain" || a.Format == "text/markdown"
                       select a.Documentation;
            if (docs.Any())
                return string.Join(Environment.NewLine, docs);
            else
                return null;
        }
        public static string GetRestDocumentation(this MethodInfo method)
            => method.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();
        public static string GetRestDocumentation(this ParameterInfo method)
            => method.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();
        public static string GetRestDocumentation(this PropertyInfo method)
            => method.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();
        public static string GetRestDocumentation(this Type type)
            => type.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();

        public static IEnumerable<RestMetaDataAttribute> GetRestMetaDataAttributes(this MethodInfo method)
            => method.GetCustomAttributes<RestMetaDataAttribute>();
        public static IEnumerable<RestMetaDataInAttribute> GetRestMetaDataInAttributes(this MethodInfo method)
            => method.GetCustomAttributes<RestMetaDataInAttribute>();
        public static IEnumerable<RestMetaDataOutAttribute> GetRestMetaDataOutAttributes(this MethodInfo method)
            => method.GetCustomAttributes<RestMetaDataOutAttribute>();
        public static bool HasMetaDataInAttribute<T>(this MethodInfo method)
            => method.GetRestMetaDataInAttributes().Any(a => a.Type == typeof(T));
        public static bool HasMetaDataOutAttribute<T>(this MethodInfo method)
            => method.GetRestMetaDataOutAttributes().Any(a => a.Type == typeof(T));
        public static IEnumerable<PropertyInfo> GetRestParameterProperties(this MethodInfo method)
            => method.GetCustomAttributes<RestParameterAttribute>()
                .SelectMany(a => a.Type.GetProperties());

        public static bool IsRequired(this PropertyInfo pi)
        {
            return pi.CanWrite && pi.CanRead && pi.GetCustomAttributes<RequiredAttribute>().Any()
                || !pi.CanWrite && pi.CanRead
                    && pi.DeclaringType.GetConstructors()
                        .Where(c => !c.IsStatic)
                        .SelectMany(c => c.GetParameters())
                        .Where(p => string.Equals(p.Name, pi.Name, StringComparison.InvariantCultureIgnoreCase))
                        .SelectMany(p => p.GetCustomAttributes<OptionalAttribute>())
                        .Any();
        }
 
 
    }
}
