using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest
{
    public interface IRestRepository<T>
    {
        IEnumerable<RestCapabilityDescriptor> GetCapabilities();
        IRestCapability<T> GetCapability(RestCapabilityDescriptorKey capability);
        C GetCapability<C>()
            where C : class, IRestCapability<T>;
    }

}
