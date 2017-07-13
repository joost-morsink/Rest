using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    [Capability("PUT")]
    public interface IRestPut<T> : IRestCapability<T>
        where T : class
    {
        ValueTask<RestResult<T>> Put(IIdentity<T> target, T entity);
    }
}
