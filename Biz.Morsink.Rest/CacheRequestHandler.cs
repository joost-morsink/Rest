using Biz.Morsink.Rest.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A Request Handler implementing the Rest caching aspect.
    /// </summary>
    public class CacheRequestHandler : IRestRequestHandler
    {
        private readonly RestRequestHandlerDelegate next;
        private readonly IRestCache cache;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next handler in the pipeline.</param>
        /// <param name="cache">An IRestCache implementation.</param>
        public CacheRequestHandler(RestRequestHandlerDelegate next, IRestCache cache)
        {
            this.next = next;
            this.cache = cache;
        }
        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="request">The Rest request.</param>
        /// <returns>An asynchronous Rest response.</returns>
        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            if (request.Capability == "GET")
            {
                var cacheResult = await cache.GetCachedResult(request);
                if (cacheResult.IsSuccesful)
                    return cacheResult.Response;
                else
                {
                    var response = await next(request);
                    if (response.Metadata.TryGet(out ResponseCaching caching)
                        && caching.StoreAllowed && caching.CacheAllowed)
                        await cache.SetCachedResult(request, response);
                    return response;
                }
            }
            else
            {
                if (request.Capability == "PUT" || request.Capability == "DELETE" || request.Capability == "PATCH")
                    await cache.ClearCachedResult(request);
                return await next(request);
            }
        }
    }
}
