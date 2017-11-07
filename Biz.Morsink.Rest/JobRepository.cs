using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;
using System.Threading;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Repository for Rest jobs.
    /// </summary>
    public class JobRepository : RestRepository<RestJob>, IRestGet<RestJob, NoParameters>
    {
        private readonly IRestJobStore restJobStore;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="restJobStore">An IRestJobStore implementation for retrieving RestJobs.</param>
        public JobRepository(IRestJobStore restJobStore)
        {
            this.restJobStore = restJobStore;
        }

        /// <summary>
        /// Gets a RestJob with a specific id.
        /// </summary>
        /// <param name="id">The identity value for the RestJob.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest response, possibly containing the RestJob with the specified identity value.</returns>
        public async ValueTask<RestResponse<RestJob>> Get(IIdentity<RestJob> id, NoParameters parameters, CancellationToken cancellationToken)
        {
            var res = await restJobStore.GetJob(id);
            if (res == null)
                return RestResult.NotFound<RestJob>().ToResponse();

            return res.Task.Status >= TaskStatus.RanToCompletion
                ? Rest.ValueBuilder(res)
                    .WithLink(Link.Create("result", new RestJobResult(res).Id))
                    .BuildResponse()
                : Rest.Value(res).ToResponse();
        }
    }

}
