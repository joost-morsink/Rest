using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface specification for Rest caches.
    /// </summary>
    public interface IRestCache
    {
        /// <summary>
        /// Tries to gets a response from the cache.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An asynchronous CacheResult.</returns>
        ValueTask<CacheResult> GetCachedResult(RestRequest request);
        /// <summary>
        /// Tries to put a response in the cache.
        /// </summary>
        /// <param name="request">The request to add the entry for.</param>
        /// <param name="response">The response to cache.</param>
        /// <returns>An asynchronous value indicating the number of entries affected.</returns>
        ValueTask<int> SetCachedResult(RestRequest request, RestResponse response);
        /// <summary>
        /// Clears a cache entry.
        /// </summary>
        /// <param name="request">The request to clear the cached entries for.</param>
        /// <returns>An asynchronous value indicating the number of entries affected.</returns>
        ValueTask<int> ClearCachedResult(RestRequest request);
    }
}
