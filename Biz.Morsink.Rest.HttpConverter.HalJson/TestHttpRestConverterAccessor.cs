using Biz.Morsink.Rest.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Biz.Morsink.Rest.HttpConverter.HalJson.Test
{
    public class TestHttpRestConverterAccessor : ICurrentHttpRestConverterAccessor
    {
        private readonly IServiceProvider serviceProvider;

        public TestHttpRestConverterAccessor(IServiceProvider sp)
        {
            serviceProvider = sp;
        }
        public IHttpRestConverter CurrentHttpRestConverter => serviceProvider.GetRequiredService<IHttpRestConverter>();
    }
}
