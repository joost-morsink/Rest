using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This request handler registers long running requests to the Rest job store.
    /// </summary>
    public class ResponsePendingRequestHandler : IRestRequestHandler
    {
        private readonly RestRequestHandlerDelegate next;
        private readonly TimeSpan maxWait;
        private readonly IRestJobStore restJobStore;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next handler in the pipeline.</param>
        /// <param name="maxWait">The maximum amount of time to wait on the response before converting it to a Pending result.</param>
        /// <param name="restJobStore">The Rest job store.</param>
        public ResponsePendingRequestHandler(RestRequestHandlerDelegate next, TimeSpan maxWait, IRestJobStore restJobStore)
        {
            this.next = next;
            this.maxWait = maxWait;
            this.restJobStore = restJobStore;
        }
        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="request">The Rest request.</param>
        /// <returns>An asynchronous Rest response.</returns>
        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            var resp = next(request).AsTask();
            await Task.WhenAny(resp, Task.Delay(maxWait));
            if (resp.Status < TaskStatus.RanToCompletion)
            {
                var job = restJobStore.RegisterJob(resp);
                return RestResult.Pending<object>(job).ToResponse();
            }
            else
                return await resp;
        }
    }
}
