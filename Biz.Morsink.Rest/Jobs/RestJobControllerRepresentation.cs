using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Jobs
{
    /// <summary>
    /// Representation class for RestJobControllers.
    /// </summary>
    public class RestJobControllerRepresentation : SimpleTypeRepresentation<RestJobController, RestJobControllerRepresentation.Representation>
    {
        private readonly IRestJobStore store;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="store">The store to construct controllers for.</param>
        public RestJobControllerRepresentation(IRestJobStore store)
        {
            this.store = store;
        }
        public override RestJobController GetRepresentable(Representation representation)
            => new RestJobController(store, representation.Id);

        public override Representation GetRepresentation(RestJobController item)
            => new Representation(item.Id);

        /// <summary>
        /// The actual representation for RestJobController.
        /// </summary>
        public class Representation
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="id">The id of the controller.</param>
            /// <param name="jobId">An optional id for the controller's parent job. This can be inferred from the controller's id.</param>
            public Representation(IIdentity<RestJob, RestJobController> id, IIdentity<RestJob> jobId = null)
            {
                jobId = jobId ?? id.Parent;
                if (!id.Provider.Equals(id.Parent, jobId))
                    throw new ArgumentException("Id does not belong to job.");
                Id = id;
                JobId = jobId;
            }
            /// <summary>
            /// The identity value for the RestJobController.
            /// </summary>
            public IIdentity<RestJob, RestJobController> Id { get; }
            /// <summary>
            /// The identity value for the RestJobController's parent RestJob.
            /// </summary>
            public IIdentity<RestJob> JobId { get; }
        }
    }
}
