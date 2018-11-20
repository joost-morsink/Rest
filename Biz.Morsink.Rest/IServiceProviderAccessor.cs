using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IServiceProviderAccessor
    {
        IServiceProvider ServiceProvider { get; }
    }
}
