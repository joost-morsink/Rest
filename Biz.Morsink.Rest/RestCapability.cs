using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public struct RestCapability<T>
    {
        public RestCapability(RestCapabilityDescriptor descriptor, IRestCapability<T> instance)
        {
            Descriptor = descriptor;
            Instance = instance;
        }
        public RestCapabilityDescriptor Descriptor { get; }
        public IRestCapability<T> Instance { get; }
        public Delegate CreateDelegate()
            => Descriptor.CreateDelegate(Instance);
    }
}
