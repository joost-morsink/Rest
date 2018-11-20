using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    public class ServiceProviderAccessor : IServiceProviderAccessor
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public ServiceProviderAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public IServiceProvider ServiceProvider => httpContextAccessor.HttpContext.RequestServices;
    }
}
