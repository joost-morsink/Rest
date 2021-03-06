﻿using Biz.Morsink.Rest.Jobs;
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
        private readonly IServiceProviderAccessor serviceProviderAccessor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next handler in the pipeline.</param>
        /// <param name="maxWait">The maximum amount of time to wait on the response before converting it to a Pending result.</param>
        /// <param name="restJobStore">The Rest job store.</param>
        public ResponsePendingRequestHandler(RestRequestHandlerDelegate next, TimeSpan maxWait, IServiceProviderAccessor serviceProviderAccessor,IRestJobStore restJobStore)
        {
            this.next = next;
            this.maxWait = maxWait;
            this.restJobStore = restJobStore;
            this.serviceProviderAccessor = serviceProviderAccessor;
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
                var user = (IUser)serviceProviderAccessor.ServiceProvider?.GetService(typeof(IUser));
                var job = await restJobStore.RegisterJob(resp, user?.Principal.Identity.Name);
                return RestResult.Pending<object>(job).ToResponse();
            }
            else
                return await resp;
        }
    }
}
