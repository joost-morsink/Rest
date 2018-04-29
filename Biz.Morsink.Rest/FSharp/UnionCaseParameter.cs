using System;

namespace Biz.Morsink.Rest.FSharp
{
    public class UnionCaseParameter
    {
        internal UnionCaseParameter(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public Type Type { get; }
        public string Name { get; }
    }
}