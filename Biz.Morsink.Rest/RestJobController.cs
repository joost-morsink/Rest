using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A Rest job controller controls the state of a RestJob.
    /// It is identified by the identity value of the RestJob, and a 'private' component for security reasons.
    /// </summary>
    public class RestJobController : IHasIdentity<RestJobController>
    {
        private readonly IRestJobStore store;
        /// <summary>
        /// Gets the identity value for the controller.
        /// </summary>
        public IIdentity<RestJob, RestJobController> Id { get; }
        /// <summary>
        /// Gets the identity for the job.
        /// </summary>
        public IIdentity<RestJob> JobId => Id.Parent;

        IIdentity<RestJobController> IHasIdentity<RestJobController>.Id => Id;
        IIdentity IHasIdentity.Id => Id;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="store">The store the controller belongs to.</param>
        /// <param name="identity">The identity value of the controller.</param>
        public RestJobController(IRestJobStore store, IIdentity<RestJob, RestJobController> identity)
        {
            this.store = store;
            Id = identity;
        }
        /// <summary>
        /// Finishes the job.
        /// </summary>
        /// <param name="value">The return value for the job.</param>
        public void Finish(object value)
        {
            store.FinishJob(this, value);
        }
    }
}
