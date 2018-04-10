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

        /// <summary>
        /// Gets a rest documentation string for a collection of RestDocumentationAttributes.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns>A rest documentation string.</returns>
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
        /// <summary>
        /// Gets a rest documentation string for the MethodInfo.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A rest documentation string.</returns>
        public static string GetRestDocumentation(this MethodInfo method)
            => method.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();
        /// <summary>
        /// Gets a rest documentation string for the ParameterInfo.
        /// </summary>
        /// <param name="parameter">The ParameterInfo.</param>
        /// <returns>A rest documentation string.</returns>
        public static string GetRestDocumentation(this ParameterInfo parameter)
            => parameter.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();
        /// <summary>
        /// Gets a rest documentation string for the PropertyInfo.
        /// </summary>
        /// <param name="property">The PropertyInfo.</param>
        /// <returns>A rest documentation string.</returns>
        public static string GetRestDocumentation(this PropertyInfo property)
            => property.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();
        /// <summary>
        /// Gets a rest documentation string for the Type.
        /// </summary>
        /// <param name="type">The Type.</param>
        /// <returns>A rest documentation string.</returns>
        public static string GetRestDocumentation(this Type type)
            => type.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();

        /// <summary>
        /// Gets a collection of RestMetaDataAttributes for the MethodInfo.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A collection of RestMetaDataAttributes.</returns>
        public static IEnumerable<RestMetaDataAttribute> GetRestMetaDataAttributes(this MethodInfo method)
            => method.GetCustomAttributes<RestMetaDataAttribute>();
        /// <summary>
        /// Gets a collection of RestMetaDataInAttributes for the MethodInfo.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A collection of RestMetaDataInAttributes.</returns>
        public static IEnumerable<RestMetaDataInAttribute> GetRestMetaDataInAttributes(this MethodInfo method)
            => method.GetCustomAttributes<RestMetaDataInAttribute>();
        /// <summary>
        /// Gets a collection of RestMetaDataOutAttributes for the MethodInfo.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A collection of RestMetaDataOutAttributes.</returns>
        public static IEnumerable<RestMetaDataOutAttribute> GetRestMetaDataOutAttributes(this MethodInfo method)
            => method.GetCustomAttributes<RestMetaDataOutAttribute>();

        /// <summary>
        /// Checks whether the MethodInfo has any RestMetaDataInAttributes for some type.
        /// </summary>
        /// <typeparam name="T">The type to check for in the in-attributes.</typeparam>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>True if the MethodInfo has any RestMetaDataInAttributes for type T.</returns>
        public static bool HasMetaDataInAttribute<T>(this MethodInfo method)
            => method.GetRestMetaDataInAttributes().Any(a => a.Type == typeof(T));
        /// <summary>
        /// Checks whether the MethodInfo has any RestMetaDataOutAttributes for some type.
        /// </summary>
        /// <typeparam name="T">The type to check for in the out-attributes.</typeparam>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>True if the MethodInfo has any RestMetaDataOutAttributes for type T.</returns>
        public static bool HasMetaDataOutAttribute<T>(this MethodInfo method)
            => method.GetRestMetaDataOutAttributes().Any(a => a.Type == typeof(T));

        /// <summary>
        /// Gets all PropertyInfo objects for parameter-types specified with the RestParameterAttribute.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A collection of PropertyInfo objects.</returns>
        public static IEnumerable<PropertyInfo> GetRestParameterProperties(this MethodInfo method)
            => method.GetCustomAttributes<RestParameterAttribute>()
                .SelectMany(a => a.Type.GetProperties());

        /// <summary>
        /// Checks whether a property is considered 'required' by an API.
        /// </summary>
        /// <param name="pi">The PropertyInfo.</param>
        /// <returns>True if the property is considered 'required'.</returns>
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
