using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest.Jobs
{
    /// <summary>
    /// Repository for finishing jobs.
    /// </summary>
    [RestDocumentation("Repository for finished messages on Rest Jobs.")]
    public class JobFinishedRepository : RestRepository<RestJobFinished>, IRestPost<RestJobFinished, Empty, RestJobFinished, Empty>
    {
        private IRestJobStore store;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="store">The Rest job store.</param>
        public JobFinishedRepository(IRestJobStore store)
        {
            this.store = store;
        }
        /// <summary>
        /// Messages posted to this endpoint finish the addressed Rest Job.
        /// </summary>
        /// <param name="target">An identity value for a Rest Job finished target.</param>
        /// <param name="parameters">No parameters.</param>
        /// <param name="entity">The RestJobFinished entity.</param>
        /// <param name="cancellationToken">A CancellationToken.</param>
        /// <returns>An asynchronous Rest response of the Empty type.</returns>
        [RestDocumentation("Messages posted to this endpoint finish the addressed Rest Job.")]
        public async ValueTask<RestResponse<Empty>> Post(IIdentity<RestJobFinished> target, Empty parameters, RestJobFinished entity, CancellationToken cancellationToken)
        {
            if (entity.Id != null && !target.Equals(entity.Id))
                return RestResult.BadRequest<Empty>(new object()).ToResponse();
            var controller = await store.GetController(target.For<RestJobController>());
            var success = await controller.Finish(entity.Value);
            if (success)
                return Rest.Value(new Empty()).ToResponse();
            else
                return RestResult.NotFound<Empty>().ToResponse();
        }
    }
}
