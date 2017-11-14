using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest.Jobs
{
    /// <summary>
    /// Repository for finishing jobs.
    /// </summary>
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
        public async ValueTask<RestResponse<Empty>> Post(IIdentity<RestJobFinished> target, Empty parameters, RestJobFinished entity, CancellationToken cancellationToken)
        {
            if (entity.Id != null && !target.Equals(entity.Id))
                return RestResult.BadRequest<Empty>(new object()).ToResponse();
            var controller = await store.GetController(entity.ControllerId);
            var success = await controller.Finish(entity.Value);
            if (success)
                return Rest.Value(new Empty()).ToResponse();
            else
                return RestResult.NotFound<Empty>().ToResponse();
        }
    }
}
