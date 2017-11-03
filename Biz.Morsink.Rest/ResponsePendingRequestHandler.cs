using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public class ResponsePendingRequestHandler : IRestRequestHandler
    {
        private readonly RestRequestHandlerDelegate next;
        private readonly IRestJobStore restJobStore;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next handler in the pipeline.</param>
        public ResponsePendingRequestHandler(RestRequestHandlerDelegate next, IRestJobStore restJobStore)
        {
            this.next = next;
            this.restJobStore = restJobStore;
        }

        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            var resp = next(request).AsTask();
            await Task.WhenAny(resp, Task.Delay(TimeSpan.FromSeconds(1.0)));
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
