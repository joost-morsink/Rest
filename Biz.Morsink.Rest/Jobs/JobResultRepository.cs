using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.Jobs
{
    /// <summary>
    /// Repository class for RestJobResults.
    /// </summary>
    public class JobResultRepository : RestRepository<RestJobResult>, IRestGet<RestJobResult, Empty>
    {
        private readonly IRestJobStore restJobStore;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="restJobStore">An IRestJobStore instance for retrieving RestJobs.</param>
        public JobResultRepository(IRestJobStore restJobStore)
        {
            this.restJobStore = restJobStore;
        }
        /// <summary>
        /// Gets a RestJobResult.
        /// </summary>
        /// <param name="id">The identity value of the RestJobResult.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest reponse that might contain a RestJobResult.</returns>
        public async ValueTask<RestResponse<RestJobResult>> Get(IIdentity<RestJobResult> id, Empty parameters, CancellationToken cancellationToken)
        {
            var res = await restJobStore.GetJob(id.Provider.Creator<RestJob>().Create(id.Value));
            if (res == null || res.Task.Status < TaskStatus.RanToCompletion)
                return RestResult.NotFound<RestJobResult>().ToResponse();
            else
                return Rest.Value(new RestJobResult(res)).ToResponse();
        }
    }
}
