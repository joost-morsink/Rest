using Biz.Morsink.Rest.Metadata;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore.Caching
{
    /// <summary>
    /// Implementation of the IRestCache interface for ASP.Net core memory caches.
    /// </summary>
    public class RestMemoryCache : IRestCache
    {
        #region Helper classes
        private class CacheEntry
        {
            public CacheEntry(RestRequest request, RestResponse response, DateTime expiry)
            {
                OriginalRequest = request;
                OriginalResponse = response;
                Expiry = expiry;
            }
            public RestRequest OriginalRequest { get; set; }
            public RestResponse OriginalResponse { get; set;  }
            public DateTime Expiry { get; set; }
            public bool Matches(RestRequest req)
                => OriginalRequest.Capability == req.Capability
                && OriginalRequest.Parameters == req.Parameters;
        }
        #endregion

        private readonly IMemoryCache memoryCache;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="memoryCache">The underlying memory cache.</param>
        public RestMemoryCache(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }
        /// <summary>
        /// Tries to gets a response from the cache.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An asynchronous CacheResult.</returns>
        public ValueTask<CacheResult> GetCachedResult(RestRequest request)
        {
            var entries = memoryCache.Get<List<CacheEntry>>(request.Address);
            if (entries == null)
                return new ValueTask<CacheResult>(new CacheResult());
            else
            {
                var now = DateTime.UtcNow;
                entries.RemoveAll(e => e.Expiry < now);

                var entry = entries.FirstOrDefault(e => e.Matches(request));
                if (entry == null)
                    return new ValueTask<CacheResult>(new CacheResult());
                else // Check cache validity?
                {
                    if (HasResponseCaching(entry.OriginalResponse, out var caching))
                    {
                        var cachingCopy = caching.Copy();
                        cachingCopy.Validity = entry.Expiry - DateTime.UtcNow;
                        entry.OriginalResponse =  entry.OriginalResponse.AddMetadata(cachingCopy);
                    }
                    return new ValueTask<CacheResult>(new CacheResult(entry.OriginalResponse));
                }
            }
        }
        /// <summary>
        /// Tries to put a response in the cache.
        /// </summary>
        /// <param name="request">The request to add the entry for.</param>
        /// <param name="response">The response to cache.</param>
        /// <returns>An asynchronous value indicating the number of entries affected.</returns>
        public ValueTask<int> SetCachedResult(RestRequest request, RestResponse response)
        {
            var lst = memoryCache.GetOrCreate(request.Address, ce =>
            {
                return new List<CacheEntry>();
            });
            if (HasResponseCaching(response, out var caching)
                && IsCacheable(request, response, caching))
            {
                lst.RemoveAll(ce => ce.Matches(request));
                lst.Add(new CacheEntry(request, response, DateTime.UtcNow + caching.Validity));
                return new ValueTask<int>(1);
            }
            else
                return new ValueTask<int>(0);
        }
        /// <summary>
        /// Clears a cache entry.
        /// </summary>
        /// <param name="request">The request to clear the cached entries for.</param>
        /// <returns>An asynchronous value indicating the number of entries affected.</returns>
        public ValueTask<int> ClearCachedResult(RestRequest request)
        {
            memoryCache.Remove(request.Address);
            return new ValueTask<int>(1);
        }
        /// <summary>
        /// Determines if the Response has a metadata element for response caching.
        /// </summary>
        /// <param name="response">The Rest Response.</param>
        /// <param name="caching">Out parameter for the ResponseCaching metadata element.</param>
        /// <returns>True if the metadata was found on the response.</returns>
        protected virtual bool HasResponseCaching(RestResponse response, out ResponseCaching caching)
            => response.Metadata.TryGet(out caching);
        /// <summary>
        /// Determines if the Request-Response pair is eligible for caching.
        /// </summary>
        /// <param name="request">The Rest Request.</param>
        /// <param name="response">The Rest Response.</param>
        /// <param name="caching">The Response caching metadata element.</param>
        /// <returns></returns>
        protected virtual bool IsCacheable(RestRequest request, RestResponse response, ResponseCaching caching)
            => request.Capability == "GET"
                && caching.StoreAllowed && !caching.CachePrivate;
    }
}
