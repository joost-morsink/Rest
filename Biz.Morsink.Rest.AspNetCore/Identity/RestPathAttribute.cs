using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    /// <summary>
    /// An attribute to specify Rest paths on repositories.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RestPathAttribute : Attribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path for the repository.</param>
        public RestPathAttribute(string path) : this(path, null, null) { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path for the repository.</param>
        /// <param name="componentTypes">The component resource types of the underlying identity value.</param>
        public RestPathAttribute(string path, Type[] componentTypes) : this(path, componentTypes, null) { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path for the repository.</param>
        /// <param name="componentTypes">The component resource types of the underlying identity value.</param>
        /// <param name="wildcardType">The querystring wildcard datatype, if applicable.</param>
        public RestPathAttribute(string path, Type[] componentTypes, Type[] wildcardTypes)
        {
            Path = path;
            ComponentTypes = componentTypes;
            WildcardTypes = wildcardTypes;
        }
        /// <summary>
        /// Gets the path for the repository.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Gets the component resource types of the underlying identity value.
        /// </summary>
        public Type[] ComponentTypes { get; set; }
        /// <summary>
        /// Gets a set of querystring wildcard datatypes, if applicable.
        /// </summary>
        public Type[] WildcardTypes { get; set; }
    }
}
