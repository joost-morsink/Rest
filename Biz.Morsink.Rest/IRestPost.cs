using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    [Capability("POST")]
    public interface IRestPost<T, E, R> : IRestCapability<T>
        where R : class
    {
        ValueTask<RestResult<R>> Post(IIdentity<T> target, E entity);
    }
}
