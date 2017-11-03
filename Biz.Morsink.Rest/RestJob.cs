using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A Rest Job represents an asynchronous RestResponse.
    /// </summary>
    public class RestJob : IHasIdentity<RestJob>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">An identity provider to create an identity value with.</param>
        /// <param name="task">The asynchronous Rest response.</param>
        public RestJob(IIdentityProvider provider, Task<RestResponse> task)
        {
            Id = provider.Creator<RestJob>().Create(Guid.NewGuid());
            Task = task;
            SetDate();
        }
        private async void SetDate()
        {
            try
            {
                await Task;
            }
            finally
            {
                Finished = DateTime.UtcNow;
            }
        }
        /// <summary>
        /// Gets the identity value of the RestJob.
        /// </summary>
        public IIdentity<RestJob> Id { get; }
        /// <summary>
        /// Gets the asynchronous Rest response.
        /// </summary>
        public Task<RestResponse> Task { get; }
        /// <summary>
        /// Gets the timestamp when the task finished.
        /// Null if the task is still running.
        /// </summary>
        public DateTime? Finished { get; private set; }

        IIdentity IHasIdentity.Id => Id;

        /// <summary>
        /// Gets a RestJobResult object for this RestJob.
        /// </summary>
        /// <returns></returns>
        public RestJobResult GetResult()
            => new RestJobResult(this);
    }

}
