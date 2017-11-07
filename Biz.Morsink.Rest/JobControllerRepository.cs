using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Utils;
namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Job controller repository -> work in progress
    /// </summary>
    public class JobControllerRepository : IRestGet<RestJobController, NoParameters>
    {
        private readonly IRestJobStore store;

        public JobControllerRepository(IRestJobStore store)
        {
            this.store = store;
        }

        public async ValueTask<RestResponse<RestJobController>> Get(IIdentity<RestJobController> id, NoParameters parameters, CancellationToken cancellationToken)
        {
            var ctrl = await store.GetController(id);
            if (ctrl == null)
                return RestResult.NotFound<RestJobController>().ToResponse();
            else
                return Rest.ValueBuilder(ctrl)
                    .WithLink(Link.Create("finish", id.Provider.Creator<RestJobFinished>().Create(id.Value), capability: typeof(IRestPost<RestJobFinished, NoParameters, RestJobFinished, NoParameters>)))
                    .BuildResponse();
        }
    }
}
