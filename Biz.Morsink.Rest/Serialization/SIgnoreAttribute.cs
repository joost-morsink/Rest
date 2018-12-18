using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property, Inherited = true)]
    public class SIgnoreAttribute : Attribute
    {
    }
}
