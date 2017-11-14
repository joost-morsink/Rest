using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;

namespace Biz.Morsink.Rest.Jobs
{
    /// <summary>
    /// Repository for the Job collection. 
    /// Allows for creating new Job controllers.
    /// </summary>
    public class JobCollectionRepository : RestRepository<RestJobCollection>, IRestPost<RestJobCollection, Empty, Empty, Empty>
    {
        private readonly IRestJobStore jobstore;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jobstore">A Rest job store.</param>
        public JobCollectionRepository(IRestJobStore jobstore)
        {
            this.jobstore = jobstore;
        }

        public async ValueTask<RestResponse<Empty>> Post(IIdentity<RestJobCollection> target, Empty parameters, Empty entity, CancellationToken cancellationToken)
        {
            var controller = await jobstore.CreateJob();
            return Rest.Value(new Empty()).ToResponse().WithMetadata(new CreatedResource { Address = controller.Id });
        }
    }
}
