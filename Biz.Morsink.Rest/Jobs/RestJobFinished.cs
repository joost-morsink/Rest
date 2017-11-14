using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Jobs
{
    /// <summary>
    /// This class represents the message to finish a previously started Rest Job.
    /// </summary>
    public class RestJobFinished : IHasIdentity<RestJobFinished>
    {
        /// <summary>
        /// Constructor. 
        /// </summary>
        public RestJobFinished()
        {
        }
        /// <summary>
        /// The identity of the Finished message.
        /// Has a one to one correspondence with the controller.
        /// </summary>
        public IIdentity<RestJob, RestJobController, RestJobFinished> Id { get; set; }
        /// <summary>
        /// The value of the asynchronous Rest Job.
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// The identity of the Controller.
        /// </summary>
        public IIdentity<RestJob, RestJobController> ControllerId => Id.Parent;

        IIdentity<RestJobFinished> IHasIdentity<RestJobFinished>.Id => Id;

        IIdentity IHasIdentity.Id => Id;
    }
}
