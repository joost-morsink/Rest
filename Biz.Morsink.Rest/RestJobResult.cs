using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class represents the result of a RestJob.
    /// </summary>
    public class RestJobResult : IHasIdentity<RestJobResult>
    {
        private readonly RestJob restJob;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="job">The RestJob.</param>
        public RestJobResult(RestJob job)
        {
            restJob = job ?? throw new ArgumentNullException(nameof(job));
        }
        /// <summary>
        /// Contains the Job's 
        /// </summary>
        public IIdentity<RestJob> JobId => restJob.Id;
        /// <summary>
        /// Gets the RestJob instance.
        /// </summary>
        public RestJob Job => restJob;
        /// <summary>
        /// The identity value for this RestJobResult.
        /// The underlying value of the result's identity value equals that of the original RestJob.
        /// </summary>
        public IIdentity<RestJobResult> Id => JobId.Provider.Creator<RestJobResult>().Create(JobId.Value);

        IIdentity IHasIdentity.Id => Id;
    }
}
