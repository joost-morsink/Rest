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
    /// Job controller repository -> work in progress
    /// </summary>
    [RestDocumentation("Repository for Job controllers")]
    public class JobControllerRepository : RestRepository<RestJobController>, IRestGet<RestJobController, Empty>
    {
        private readonly IRestJobStore store;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="store">A Rest Job store.</param>
        public JobControllerRepository(IRestJobStore store)
        {
            this.store = store;
        }

        /// <summary>
        /// Gets the controller for a certain Rest Job.
        /// </summary>
        /// <param name="id">An identity value for the Rest Job controller.</param>
        /// <param name="parameters">No parameters.</param>
        /// <param name="cancellationToken">A CancellationToken.</param>
        /// <returns>An asynchronous Rest response of type RestJobController.</returns>
        [RestDocumentation(@"Gets the controller for a certain Rest Job.")]
        public async ValueTask<RestResponse<RestJobController>> Get(IIdentity<RestJobController> id, Empty parameters, CancellationToken cancellationToken)
        {
            var ctrl = await store.GetController(id);
            if (ctrl == null)
                return RestResult.NotFound<RestJobController>().ToResponse();
            else 
                return Rest.ValueBuilder(ctrl)
                    .WithLink(Link.Create("finish", id.Provider.Creator<RestJobFinished>().Create((ctrl.JobId.ComponentValue, ctrl.Id.ComponentValue, "")), capability: typeof(IRestPost<RestJobFinished, Empty, RestJobFinished, Empty>)))
                    .BuildResponse();
        }
    }
}
