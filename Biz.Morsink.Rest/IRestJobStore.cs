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
    }
}