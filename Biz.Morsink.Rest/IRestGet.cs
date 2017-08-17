using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
namespace Biz.Morsink.Rest
{
    [Capability("GET")]
    public interface IRestGet<T, P> : IRestCapability<T>
        where T : class
    {
        ValueTask<RestResult<T>> Get(IIdentity<T> id, P parameters);
    }

}
