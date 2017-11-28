using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Base Attribute for Attributed Rest Repositories.
    /// </summary>
    public abstract class RestAttribute : Attribute
    {
        /// <summary>
        /// Should return the capability the attribute represents.
        /// </summary>
        public abstract string Capability { get; }
    }
    /// <summary>
    /// Attribute representing the GET capability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RestGetAttribute : RestAttribute
    {
        /// <summary>
        /// Gets the capability string "GET"
        /// </summary>
        public override string Capability => "GET";
    }
    /// <summary>
    /// Attribute representing the PUT capability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPutAttribute : RestAttribute
    {
        /// <summary>
        /// Gets the capability string "PUT"
        /// </summary>
        public override string Capability => "PUT";
    }
    /// <summary>
    /// Attribute representing the POST capability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPostAttribute : RestAttribute
    {
        /// <summary>
        /// Gets the capability string "POST"
        /// </summary>
        public override string Capability => "POST";
    }
    /// <summary>
    /// Attribute representing the PATCH capability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPatchAttribute : RestAttribute
    {
        /// <summary>
        /// Gets the capability string "PATCH"
        /// </summary>
        public override string Capability => "PATCH";
    }
    /// <summary>
    /// Attribute representing the DELETE capability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RestDeleteAttribute : RestAttribute
    {
        /// <summary>
        /// Gets the capability string "DELETE"
        /// </summary>
        public override string Capability => "DELETE";
    }
    /// <summary>
    /// This attribute indicates a method parameter is supposed to be filled with data from the Rest request's body.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RestBodyAttribute : Attribute { }
}
