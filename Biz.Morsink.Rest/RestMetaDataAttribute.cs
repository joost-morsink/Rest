using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Abstract attribute base class for the attribution of meta data types to implementation constructs.
    /// </summary>
    public abstract class RestMetaDataAttribute : Attribute 
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The metadata type.</param>
        public RestMetaDataAttribute(Type type)
        {
            Type = type;
        }
        /// <summary>
        /// The metadata type.
        /// </summary>
        public Type Type { get; }
    }
    /// <summary>
    /// Attribute for meta data types for requests.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class RestMetaDataInAttribute : RestMetaDataAttribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The metadata type.</param>
        public RestMetaDataInAttribute(Type type) : base(type) { }
    }
    /// <summary>
    /// Attribute for meta data types for responses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class RestMetaDataOutAttribute : RestMetaDataAttribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The metadata type.</param>
        public RestMetaDataOutAttribute(Type type) : base(type) { }
    }
}
