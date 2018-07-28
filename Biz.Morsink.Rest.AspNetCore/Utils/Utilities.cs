using Biz.Morsink.Rest.Schema;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    /// <summary>
    /// Utility class.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Version 1.0
        /// </summary>
        public static readonly Version VERSION_ONE = new Version(1, 0);
        /// <summary>
        /// Unescape a string into proper content
        /// </summary>
        public static string UriDecode(string str)
        {
            if (str == null)
                return null;
            var n = str.Length;
            var sb = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                if (str[i] != '%')
                    sb.Append(str[i]);
                else if (str[i + 1] == '%')
                    sb.Append(str[++i]);
                else
                {
                    sb.Append((char)int.Parse(str.Substring(i + 1, 2), System.Globalization.NumberStyles.HexNumber));
                    i += 2;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Escapes non-safe characters in a string
        /// </summary>
        public static string UriEncode(string str)
        {
            if (str == null)
                return null;
            if (!IsEscapingNeeded(str))
                return str;
            var n = str.Length;
            var sb = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                var ch = str[i];
                if (IsSafeCharacter(ch))
                    sb.Append(ch);
                else
                {
                    sb.Append('%');
                    sb.Append(BitConverter.ToString(new[] { (byte)ch }));
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Determines if escaping is needed on a string
        /// </summary>
        public static bool IsEscapingNeeded(string segment)
        {
            var n = segment.Length;
            for (int i = 0; i < n; i++)
                if (!IsSafeCharacter(segment[i]))
                    return true;
            return false;
        }
        /// <summary>
        /// Determines if a character is safe
        /// </summary>
        public static bool IsSafeCharacter(char ch)
            => ch >= '0' && ch <= '9'
            || ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z'
            || ch == '.' || ch == '-' || ch == '_' || ch == '~';

        /// <summary>
        /// Creates a RestCapabilities structure based on the parameters.
        /// </summary>
        /// <param name="idProvider">An IRestIdentityProvider for retrieving QueryString wildcard types.</param>
        /// <param name="repo">The repository to get the capabilities from.</param>
        /// <param name="typeDescriptorCreator">A TypeDescriptorCreator for the actual creation of TypeDescriptors.</param>
        /// <returns></returns>
        public static RestCapabilities MakeCapabilities(IRestIdentityProvider idProvider, IRestRepository repo, TypeDescriptorCreator typeDescriptorCreator)
        {
            var res = new RestCapabilities();
            if (repo == null)
                return res;
            var wildcardtypes = idProvider.GetRestPaths(repo.EntityType)
                .Where(rp => rp.QueryString.IsWildcard)
                .Select(rp => rp.QueryString.WildcardTypes)
                .Where(t => t != null)
                .FirstOrDefault();
            foreach (var capGroup in repo.GetCapabilities().GroupBy(c => c.Name))
            {
                var wildcardDescriptor = wildcardtypes == null || wildcardtypes.Length == 0
                    ? TypeDescriptor.MakeEmpty()
                    : TypeDescriptor.MakeIntersection("", wildcardtypes.Select(typeDescriptorCreator.GetDescriptor), null);

                res[capGroup.Key] = capGroup.Select(cap => new RequestDescription(
                     typeDescriptorCreator.GetDescriptor(cap.BodyType),
                     capGroup.Key == "GET" && wildcardtypes != null && wildcardtypes.Length > 0
                        ? cap.ParameterType == typeof(Empty)
                            ? wildcardDescriptor
                            : TypeDescriptor.MakeIntersection("", new[] { wildcardDescriptor, typeDescriptorCreator.GetDescriptor(cap.ParameterType) }, null)
                        : typeDescriptorCreator.GetDescriptor(cap.ParameterType),
                     typeDescriptorCreator.GetDescriptor(cap.ResultType)
                )).ToArray();
            }
            return res;
        }
        internal static IEnumerable<RestPath.Segment> GetSegments(this RestPath rp)
        {
            for (int i = 0; i < rp.Count; i++)
                yield return rp[i];
        }
        /// <summary>
        /// Checks if a certain type of object is available in the HttpContext.
        /// </summary>
        /// <typeparam name="T">The type the stored object is supposed to have.</typeparam>
        /// <param name="httpContext">The HttpContext.</param>
        /// <returns>True if the object was found, false otherwise.</returns>
        public static bool HasContextItem<T>(this HttpContext httpContext)
            => httpContext.Items.ContainsKey(typeof(T));
        /// <summary>
        /// Tries to retrieve a stored object from an HttpContext's Items collection by using the type as key.
        /// </summary>
        /// <typeparam name="T">The type the stored object is supposed to have.</typeparam>
        /// <param name="httpContext">The HttpContext.</param>
        /// <param name="item">When found, this parameter will contain the found object.</param>
        /// <returns>True if the object was found, false otherwise.</returns>
        public static bool TryGetContextItem<T>(this HttpContext httpContext, out T item)
            where T : class
        {
            var res = httpContext.Items.TryGetValue(typeof(T), out var val);
            item = val as T;
            return res;
        }
        /// <summary>
        /// Stores an object in an HttpContext's Items collection by using the type as key.
        /// The object can only be found using the exact type used here.
        /// </summary>
        /// <typeparam name="T">The type of the object, and key to use.</typeparam>
        /// <param name="httpContext">The HttpContext.</param>
        /// <param name="item">The item to store.</param>
        public static void SetContextItem<T>(this HttpContext httpContext, T item)
        {
            httpContext.Items[typeof(T)] = item;
        }
    }
}
