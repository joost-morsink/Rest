using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class provides automatic cancellation of request after a specific timeout.
    /// </summary>
    public class CancelRequestHandler : IRestRequestHandler
    {
        private readonly RestRequestHandlerDelegate next;
        private readonly TimeSpan maxWait;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next handler in the pipeline.</param>
        /// <param name="maxWait">The maximum amount of time to wait on the response before cancelling the request.</param>
        public CancelRequestHandler(RestRequestHandlerDelegate next, TimeSpan maxWait)
        {
            this.next = next;
            this.maxWait = maxWait;

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
                request.Cancel();
                return RestResult.Error<object>(new OperationCanceledException()).ToResponse();
            }
            else
                return await resp;
        }
    }
}
