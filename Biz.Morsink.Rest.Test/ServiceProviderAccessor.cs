using System;

namespace Biz.Morsink.Rest.Test
{
    public class ServiceProviderAccessor : IServiceProviderAccessor
    {
        public static ServiceProviderAccessor Instance { get; } = new ServiceProviderAccessor();

        public IServiceProvider ServiceProvider { get; set; }
    }
}
