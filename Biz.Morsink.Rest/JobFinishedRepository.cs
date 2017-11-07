using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest
{
    public class JobFinishedRepository : RestRepository<RestJobFinished>, IRestPost<RestJobFinished, NoParameters, RestJobFinished, NoParameters>
    {
        private IRestJobStore store;

        public JobFinishedRepository(IRestJobStore store)
        {
            this.store = store;
        }
        public async ValueTask<RestResponse<NoParameters>> Post(IIdentity<RestJobFinished> target, NoParameters parameters, RestJobFinished entity, CancellationToken cancellationToken)
        {
            if (!target.Equals(entity.Id))
                return RestResult.BadRequest<NoParameters>(new object()).ToResponse();
            var controller = await store.GetController(entity.GetControllerId());
            var success = await controller.Finish(entity.Value);
            if (success)
                return Rest.Value(new NoParameters()).ToResponse();
            else
                return RestResult.NotFound<NoParameters>().ToResponse();
        }
    }
}
