using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    public static class StringUtils
    {
        public static string CasedToPascalCase(this string str)
        {
            if (str.Length > 0 && str[0] == '_')
                str = str.Substring(1);
            if (str.Length > 0 && !char.IsUpper(str[0]))
                str = char.ToUpper(str[0]) + str.Substring(1);
            return str;
        }
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
