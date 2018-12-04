using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
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
        private readonly IRestIdentityProvider identityProvider;
        private readonly ITypeDescriptorCreator typeDescriptorCreator;
        private readonly CacheSerializer serializer;

        public RestDistributedCache(IDistributedCache cache, ITypeDescriptorCreator typeDescriptorCreator,  IRestIdentityProvider identityProvider)
        {
            this.cache = cache;
            this.identityProvider = identityProvider;
            this.typeDescriptorCreator = typeDescriptorCreator;
            this.serializer = new CacheSerializer(typeDescriptorCreator, identityProvider);

        }
        private string GetKey(RestRequest request)
        {
            var converter = identityProvider.GetConverter(request.Address.ForType, false);
            if (converter.Convert(request.Address.Value).TryTo(out string id))
                return string.Concat(request.Address.ForType.FullName, "|", id);
            else
                return null;
        }
        private byte[] GetValue(RestResponse restResponse)
        {
           

        }
        public ValueTask<int> ClearCachedResult(RestRequest request)
        {
            var key = GetKey(request);
            if (key == null)
                return new ValueTask<int>(0);
            else
                return remove();
            
            async ValueTask<int> remove()
            {
                await cache.RemoveAsync(key);
                return 1;
            }
        }

        public ValueTask<CacheResult> GetCachedResult(RestRequest request)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> SetCachedResult(RestRequest request, RestResponse response)
        {
            throw new NotImplementedException();
        }

        
    }
}
