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
    public class JobCollectionRepository : RestRepository<RestJobCollection>, IRestPost<RestJobCollection, JobCollectionRepository.PostParameters, Empty, Empty>
    {
        public class PostParameters
        {
            public bool Secure { get; set; }
        }
        private readonly IRestJobStore jobstore;
        private readonly IUser user;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jobstore">A Rest job store.</param>
        /// <param name="user">The user registering the Job.</param>
        public JobCollectionRepository(IRestJobStore jobstore, IUser user = null)
        {
            this.jobstore = jobstore;
            this.user = user;
        }

        public async ValueTask<RestResponse<Empty>> Post(IIdentity<RestJobCollection> target, PostParameters parameters, Empty entity, CancellationToken cancellationToken)
        {
            if (parameters.Secure && user?.Principal == null)
                return RestResult.BadRequest<Empty>("Cannot be secure without user").ToResponse();
            var controller = await jobstore.CreateJob(parameters.Secure ? user?.Principal.Identity.Name : null);
            return Rest.Value(new Empty()).ToResponse().WithMetadata(new CreatedResource { Address = controller.Id });
        }
    }
}
