using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    /// <summary>
    /// A utility class for strings.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Converts a cased string to PascalCase
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>A PascalCased version of the string.</returns>
        public static string CasedToPascalCase(this string str)
        {
            if (str.Length > 0 && str[0] == '_')
                str = str.Substring(1);
            if (str.Length > 0 && !char.IsUpper(str[0]))
                str = char.ToUpper(str[0]) + str.Substring(1);
            return str;
        }
        /// <summary>
        /// Converts a cased string to camelCase.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>A camelCased version of the string.</returns>
        public static string CasedToCamelCase(this string str)
        {
            if (str.Length > 0 && str[0] == '_')
                str = str.Substring(1);
            if (str.Length > 0 && !char.IsLower(str[0]))
                str = char.ToLower(str[0]) + str.Substring(1);
            return str;
        }

    }
}
