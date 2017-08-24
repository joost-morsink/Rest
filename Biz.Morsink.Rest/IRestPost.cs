using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    [Capability("POST")]
    public interface IRestPost<T, P, E, R> : IRestCapability<T>
        where R : class
    {
        ValueTask<RestResponse<R>> Post(IIdentity<T> target, P parameters, E entity);
    }
}
