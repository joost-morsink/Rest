using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest
{
    public interface IRestRepository
    {
        IEnumerable<RestCapabilityDescriptor> GetCapabilities();
        Type EntityType { get; }
    }
    public interface IRestRepository<T> : IRestRepository
    {
        IRestCapability<T> GetCapability(RestCapabilityDescriptorKey capability);
        C GetCapability<C>()
            where C : class, IRestCapability<T>;
    }

}
