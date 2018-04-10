using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;
using System.Threading;

namespace Biz.Morsink.Rest.Jobs
{
    /// <summary>
    /// Repository for Rest jobs.
    /// </summary>
    [RestDocumentation("Rest Job repository.")]
    public class JobRepository : RestRepository<RestJob>, IRestGet<RestJob, Empty>
    {
        private readonly IRestJobStore restJobStore;
        private readonly IUser user;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="restJobStore">An IRestJobStore implementation for retrieving RestJobs.</param>
        /// <param name="user">The user registering the Job.</param>
        public JobRepository(IRestJobStore restJobStore, IUser user = null)
        {
            this.restJobStore = restJobStore;
            this.user = user;
        }

        /// <summary>
        /// Gets a RestJob with a specific id.
        /// </summary>
        /// <param name="id">The identity value for the RestJob.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest response, possibly containing the RestJob with the specified identity value.</returns>
        [RestDocumentation("Gets the status for some Rest Job.")]
        public async ValueTask<RestResponse<RestJob>> Get(IIdentity<RestJob> id, Empty parameters, CancellationToken cancellationToken)
        {
            var res = await restJobStore.GetJob(id);
            if (res == null || res.User != null && !string.Equals(res.User, user?.Principal.Identity.Name, StringComparison.InvariantCultureIgnoreCase))
                return RestResult.NotFound<RestJob>().ToResponse();

            return res.Task.Status >= TaskStatus.RanToCompletion
                ? Rest.ValueBuilder(res)
                    .WithLink(Link.Create("result", new RestJobResult(res).Id))
                    .BuildResponse()
                : Rest.Value(res).ToResponse();
        }
    }

}
