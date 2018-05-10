using System;
using System.Reflection;

namespace Biz.Morsink.Rest.FSharp
{
    /// <summary>
    /// This class describes a parameter to some F# union type case.
    /// </summary>
    public class UnionCaseParameter
    {
        internal UnionCaseParameter(Type type, string name, PropertyInfo property)
        {
            Type = type;
            Name = name;
            Property = property;
        }

        /// <summary>
        /// Contains the type of the parameter.
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// Contains the name of the parameter.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Contains the PropertyInfo that can be used to retrieve the parameter's value from a case's instance.
        /// </summary>
        public PropertyInfo Property { get; }
    }
}