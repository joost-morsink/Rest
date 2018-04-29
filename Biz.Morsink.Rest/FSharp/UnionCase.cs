using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Biz.Morsink.Rest.FSharp
{
    public class UnionCase
    {
        internal static UnionCase Create((MethodInfo,int) m)
        {
            return new UnionCase(m.Item2,
                m.Item1.GetParameters().Select(p => new UnionCaseParameter(p.ParameterType, adjustName(p.Name))));

            string adjustName(string str)
            {
                if (str.Length > 0 && str[0] == '_')
                    str = str.Substring(1);
                if (str.Length > 0 && !char.IsUpper(str[0]))
                    str = char.ToUpper(str[0]) + str.Substring(1);
                return str;
            }
        }
        internal UnionCase(int tag, IEnumerable<UnionCaseParameter> parameters)
        {
            Tag = tag;
            Parameters = parameters.ToArray();
        }
        public int Tag { get;  }
        public IReadOnlyList<UnionCaseParameter> Parameters { get; }
    }
}