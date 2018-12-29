using Biz.Morsink.Rest.Utils;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter
{
    /// <summary>
    /// A default naming strategy for Json serialization.
    /// </summary>
    public class DefaultNamingStrategy : NamingStrategy
    {

        protected override string ResolvePropertyName(string name)
        {
            // If all characters are upper case, leave it as is.
            if (name.All(char.IsUpper))
                return name;
            else // otherwise apply camel casing
                return name.CasedToCamelCase();
        }
    }
}
