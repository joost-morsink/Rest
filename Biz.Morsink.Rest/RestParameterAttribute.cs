using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Attribute for indicating hidden or dynamically typed parameter types on rest implementation constructs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter,
        AllowMultiple = true, Inherited = true)]
    public class RestParameterAttribute : Attribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The parameter type.</param>
        public RestParameterAttribute(Type type)
        {
            Type = type;
        }
        /// <summary>
        /// The parameter type.
        /// </summary>
        public Type Type { get; }
    }
}
