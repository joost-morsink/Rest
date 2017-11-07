using Biz.Morsink.Identity;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Specifies a store for asynchronous Rest responses (Rest jobs)
    /// </summary>
    public interface IRestJobStore
    {
        /// <summary>
        /// Gets a RestJob from the store.
        /// </summary>
        /// <param name="id">The identity value of the job.</param>
        /// <returns>The RestJob if it is present in the store, null otherwise.</returns>
        RestJob GetJob(IIdentity<RestJob> id);
        /// <summary>
        /// Registers an asynchronous Rest response as a Rest job.
        /// </summary>
        /// <param name="task">The asynchronous Rest response.</param>
        /// <returns>The RestJob as registered in the store.</returns>
        RestJob RegisterJob(Task<RestResponse> task);

        /// <summary>
        /// Gets a job controller from the job store.
        /// </summary>
        /// <param name="id">The identity value for the job controller.</param>
        /// <returns>A RestJobController instance if it was found, null otherwise.</returns>
        RestJobController GetController(IIdentity<RestJob, RestJobController> id);
        /// <summary>
        /// Creates a RestJob.
        /// </summary>
        /// <returns>A controller for the RestJob.</returns>
        RestJobController CreateJob();
        /// <summary>
        /// Finishes a RestJob corresponding to the controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="value">The result value for the job.</param>
        void FinishJob(RestJobController controller, object value);
    }
}