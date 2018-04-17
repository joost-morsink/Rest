using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    public static class Utilities
    {
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
                else if (ch == '%')
                    sb.Append("%%");
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
            || ch == '-' || ch == '_' || ch == '~';

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
            var wildcardtypes = idProvider.GetRestPaths(repo.EntityType)
                .Where(rp => rp.QueryString.IsWildcard)
                .Select(rp => rp.QueryString.WildcardTypes)
                .Where(t => t != null)
                .FirstOrDefault();
            foreach (var capGroup in repo.GetCapabilities().GroupBy(c => c.Name))
            {
                var wildcardDescriptor = wildcardtypes == null || wildcardtypes.Length == 0
                    ? TypeDescriptor.MakeEmpty()
                    : TypeDescriptor.MakeIntersection("", wildcardtypes.Select(typeDescriptorCreator.GetDescriptor));

                res[capGroup.Key] = capGroup.Select(cap => new RequestDescription(
                     typeDescriptorCreator.GetDescriptor(cap.BodyType),
                     capGroup.Key == "GET" && wildcardtypes != null && wildcardtypes.Length > 0
                        ? cap.ParameterType == typeof(Empty)
                            ? wildcardDescriptor
                            : TypeDescriptor.MakeIntersection("", new[] { wildcardDescriptor, typeDescriptorCreator.GetDescriptor(cap.ParameterType) })
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
    }
}
