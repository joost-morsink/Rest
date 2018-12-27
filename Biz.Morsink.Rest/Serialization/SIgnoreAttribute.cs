using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// Attribute to indicate a property or constructor is to be ignored for purposes of serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property, Inherited = true)]
    public class SIgnoreAttribute : Attribute
    {
    }
}
