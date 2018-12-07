using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO;
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

        public RestDistributedCache(IDistributedCache cache, ITypeDescriptorCreator typeDescriptorCreator, IRestIdentityProvider identityProvider)
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
        private async ValueTask<byte[]> GetValue(RestResponse restResponse)
        {
            var bf = new SBinaryFormatter();
            var item = serializer.Serialize(Serialization.SerializationContext.Create(identityProvider), (object)restResponse);
            using (var ms = new MemoryStream())
            {
                await bf.WriteItem(ms, item);
                return ms.ToArray();
            }
        }
        private interface IResponseSerializer
        {
            ValueTask<RestResponse> GetResponse(byte[] bytes);
        }
        private class ResponseSerializer<T> : IResponseSerializer
        {
            private readonly RestDistributedCache parent;

            public ResponseSerializer(RestDistributedCache parent)
            {
                this.parent = parent;
            }

            public async ValueTask<RestResponse<T>> GetResponse(byte[] bytes)
            {
                var bf = new SBinaryFormatter();
                using (var ms = new MemoryStream(bytes))
                {
                    var item = await bf.ReadItem(ms);
                    return parent.serializer.Deserialize<RestResponse<T>>(Serialization.SerializationContext.Create(parent.identityProvider), item);
                }
            }
            async ValueTask<RestResponse> IResponseSerializer.GetResponse(byte[] bytes)
                => await GetResponse(bytes);
        }
        private ValueTask<RestResponse> GetResponse(byte[] bytes, Type type)
        {
            var ser = (IResponseSerializer)Activator.CreateInstance(typeof(ResponseSerializer<>).MakeGenericType(type), this);
            return ser.GetResponse(bytes);
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

        public async ValueTask<CacheResult> GetCachedResult(RestRequest request)
        {
            var key = GetKey(request);
            var bytes = await cache.GetAsync(key);
            if (bytes == null)
                return new CacheResult();
            else
                return new CacheResult(await GetResponse(bytes, request.Address.ForType));
        }

        public async ValueTask<int> SetCachedResult(RestRequest request, RestResponse response)
        {
            var key = GetKey(request);
            var bytes = await GetValue(response);
            await cache.SetAsync(key, bytes);
            return 1;
        }


    }
}
