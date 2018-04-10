using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter,
        AllowMultiple = true, Inherited = true)]
    public class RestParameterAttribute : Attribute
    {
        public RestParameterAttribute(Type type)
        {
            Type = type;
        }
        public Type Type { get; }
    }
}
