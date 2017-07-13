using System;
using System.Collections.Generic;

namespace Biz.Morsink.Rest
{
    public interface IRestRepository<T>
    {
        IEnumerable<Type> GetCapabilities();
        IRestCapability<T> GetCapability(Type capability);
        C GetCapability<C>()
            where C : class, IRestCapability<T>;
    }

}
