using Biz.Morsink.Identity;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore.Caching
{
    public class RestDistributedCache : IRestCache
    {
        private readonly IDistributedCache cache;

        public RestDistributedCache(IDistributedCache cache)
        {
            this.cache = cache;
        }
        private string Key(IIdentity id)
            => id.ToString();
        public async ValueTask<int> ClearCachedResult(RestRequest request)
        {
            await cache.RemoveAsync(Key(request.Address));
            return 1;
        }

        public async ValueTask<CacheResult> GetCachedResult(RestRequest request)
        {
            var entry = await cache.GetStringAsync(Key(request.Address));
            return new CacheResult();
        }

        public ValueTask<int> SetCachedResult(RestRequest request, RestResponse response)
        {
            return new ValueTask<int>(0);
        }
    }
}
