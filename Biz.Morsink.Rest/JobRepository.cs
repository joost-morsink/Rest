using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;

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

            return res.Task.Status >= TaskStatus.RanToCompletion
                ? Rest.ValueBuilder(res)
                    .WithLink(Link.Create("result", res.Id.Provider.Creator<RestJobResult>().Create(res.Id.Value)))
                    .BuildResponseAsync()
                : Rest.Value(res).ToResponseAsync();
        }
    }

}
