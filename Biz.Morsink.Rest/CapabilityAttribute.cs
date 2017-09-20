using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Attribute to add a metadata identifier for a rest capability on an interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class CapabilityAttribute : Attribute
    {
        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="name">The capability's name</param>
        public CapabilityAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Gets the capability's name.
        /// </summary>
        public string Name { get; }
    }
}
