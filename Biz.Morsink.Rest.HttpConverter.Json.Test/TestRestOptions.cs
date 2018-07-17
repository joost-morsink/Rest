using Biz.Morsink.Rest.AspNetCore;
using Microsoft.Extensions.Options;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    public class TestRestOptions : IOptions<RestAspNetCoreOptions>
    {
        public RestAspNetCoreOptions Value => new RestAspNetCoreOptions
        {
            UseCuries = false
        };
    }
}
