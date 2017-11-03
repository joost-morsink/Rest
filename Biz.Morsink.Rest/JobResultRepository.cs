using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public class JobResultRepository : RestRepository<RestJobResult>, IRestGet<RestJobResult, NoParameters>
    {
        private readonly IRestJobStore restJobStore;
        public JobResultRepository(IRestJobStore restJobStore)
        {
            this.restJobStore = restJobStore;
        }

        public ValueTask<RestResponse<RestJobResult>> Get(IIdentity<RestJobResult> id, NoParameters parameters)
        {
            var res = restJobStore.GetJob(id.Provider.Creator<RestJob>().Create(id.Value));
            if (res == null || res.Task.Status < TaskStatus.RanToCompletion)
                return RestResult.NotFound<RestJobResult>().ToResponseAsync();
            else
                return Rest.Value(new RestJobResult(res)).ToResponseAsync();
        }

        public override async ValueTask<RestResponse> ProcessResponse(RestResponse response)
        {
            if (response.UntypedResult.AsSuccess()?.RestValue.Value is RestJobResult result)
            {
                var job = result.Job;
                if (job.Task.IsCompleted)
                    return await job.Task;
                else
                    return RestResult.Pending<object>(job).ToResponse().WithMetadata(new ResponseCaching { CacheAllowed = false, CachePrivate = true, StoreAllowed = false });
            }
            else
                return await base.ProcessResponse(response);
        }
    }
}
