using System;
using System.Reflection;

namespace Biz.Morsink.Rest.FSharp
{
    public class UnionCaseParameter
    {
        internal UnionCaseParameter(Type type, string name, PropertyInfo property)
        {
            Type = type;
            Name = name;
            Property = property;
        }

        public Type Type { get; }
        public string Name { get; }
        public PropertyInfo Property { get; }
    }
}