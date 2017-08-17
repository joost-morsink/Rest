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
        IReadOnlyList<RestCapability<T>> GetCapabilities(RestCapabilityDescriptorKey capability);
        C GetCapability<C>()
            where C : class, IRestCapability<T>;
    }

}
