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
    [RestDocumentation("Repository for Rest Job results.")]
    public class JobResultRepository : RestRepository<RestJobResult>, IRestGet<RestJobResult, Empty>
    {
        private readonly IRestJobStore restJobStore;
        private readonly IUser user;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="restJobStore">An IRestJobStore instance for retrieving RestJobs.</param>
        /// <param name="user">The user registering the Job.</param>
        public JobResultRepository(IRestJobStore restJobStore, IUser user)
        {
            this.restJobStore = restJobStore;
            this.user = user;
        }
        /// <summary>
        /// Gets a RestJobResult.
        /// </summary>
        /// <param name="id">The identity value of the RestJobResult.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest reponse that might contain a RestJobResult.</returns>
        [RestDocumentation("Gets the result of a finished Rest Job. The result is packaged in a wrapper object.")]
        public async ValueTask<RestResponse<RestJobResult>> Get(IIdentity<RestJobResult> id, Empty parameters, CancellationToken cancellationToken)
        {
            var res = await restJobStore.GetJob(id.Provider.Creator<RestJob>().Create(id.Value));
            if (res == null || res.Task.Status < TaskStatus.RanToCompletion || res.User != null && !string.Equals(res.User, user?.Principal.Identity.Name, StringComparison.InvariantCultureIgnoreCase))
                return RestResult.NotFound<RestJobResult>().ToResponse();
            else
                return Rest.Value(new RestJobResult(res)).ToResponse();
        }
    }
}
