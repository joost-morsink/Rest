using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    [Capability("PUT")]
    public interface IRestPut<T, P> : IRestCapability<T>
        where T : class
    {
        ValueTask<RestResponse<T>> Put(IIdentity<T> target, P parameters, T entity);
    }
}
