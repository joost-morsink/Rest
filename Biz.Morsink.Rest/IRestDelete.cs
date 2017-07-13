using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    [Capability("DELETE")]
    public interface IRestDelete<T> : IRestCapability<T>
    {
        ValueTask<RestResult<object>> Delete(IIdentity<T> target);
    }
}
