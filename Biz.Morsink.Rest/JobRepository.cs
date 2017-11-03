using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest
{
    public class JobRepository : RestRepository<RestJob>, IRestGet<RestJob, NoParameters>
    {
        private readonly IRestJobStore restJobStore;

        public JobRepository(IRestJobStore restJobStore)
        {
            this.restJobStore = restJobStore;
        }

        public ValueTask<RestResponse<RestJob>> Get(IIdentity<RestJob> id, NoParameters parameters)
        {
            var res = restJobStore.GetJob(id);
            if (res == null)
                return RestResult.NotFound<RestJob>().ToResponseAsync();

            return Rest.Value(res).ToResponseAsync();
        }

        public override async ValueTask<RestResponse> ProcessResponse(RestResponse response)
        {
            if (response.UntypedResult.AsSuccess()?.RestValue.Value is RestJob job)
            {
                if (job.Task.IsCompleted)
                    return await job.Task;
                else
                    return RestResult.Pending<object>(job).ToResponse();
            }
            else
                return await base.ProcessResponse(response);
        }
    }
}
