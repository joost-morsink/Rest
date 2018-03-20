using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    public class RestAspNetCoreOptions : IOptions<RestAspNetCoreOptions>
    {
        public RestAspNetCoreOptions() { }
        public bool UseCuries { get; set; }
        
        RestAspNetCoreOptions IOptions<RestAspNetCoreOptions>.Value => this;
    }
}
