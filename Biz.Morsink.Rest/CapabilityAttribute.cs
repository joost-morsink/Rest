using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class CapabilityAttribute : Attribute
    {
        public CapabilityAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }
}
