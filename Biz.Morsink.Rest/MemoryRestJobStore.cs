using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using Biz.Morsink.DataConvert;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class provides an implementation of the IRestJobStore interface with an in-memory datastructure.
    /// </summary>
    public class MemoryRestJobStore : IRestJobStore, IDisposable
    {
        #region Helper classes
        private class Entry
        {
            public Entry(string key, RestJob job)
            {
                Key = key ?? throw new ArgumentNullException(nameof(key));
                Job = job ?? throw new ArgumentNullException(nameof(job));
            }
            public string Key { get; }
            public RestJob Job { get; }

        }
        private class ManualEntry
        {
            public ManualEntry(IIdentity<RestJob, RestJobController> id)
            {
                Id = id;
                taskCompletionSource = new TaskCompletionSource<RestResponse>();
                Response = taskCompletionSource.Task;
            }

            public IIdentity<RestJob, RestJobController> Id { get; }

            private readonly TaskCompletionSource<RestResponse> taskCompletionSource;

            public Task<RestResponse> Response { get; }
            public void Finish(object value)
            {
                taskCompletionSource.SetResult(Rest.Value(value).ToResponse());
            }
        }
        #endregion

        private readonly ConcurrentDictionary<string, Entry> entries;
        private readonly ConcurrentDictionary<string, ManualEntry> manualEntries;
        private readonly CancellationTokenSource cancel;
        private readonly Task scavengeTask;
        private readonly TimeSpan maxAge;
        private readonly IIdentityProvider identityProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identityProvider">An identity provider to create RestJob identities with.</param>
        /// <param name="maxAge">The maximum age of finished responses. Default 1 hour.</param>
        public MemoryRestJobStore(IIdentityProvider identityProvider, TimeSpan? maxAge = null)
        {
            entries = new ConcurrentDictionary<string, Entry>();
            manualEntries = new ConcurrentDictionary<string, ManualEntry>();
            cancel = new CancellationTokenSource();
            this.maxAge = maxAge ?? TimeSpan.FromHours(1.0);
            this.identityProvider = identityProvider;
            scavengeTask = StartScavenging(cancel.Token);
        }

        private async Task StartScavenging(CancellationToken token)
        {
            try
            {
                do
                {
                    var now = DateTime.UtcNow;
                    var keys = entries.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        if (entries.TryGetValue(key, out var entry) && entry.Job.Finished.HasValue && now - entry.Job.Finished.Value > maxAge)
                        {
                            entries.TryRemove(key, out entry);
                            manualEntries.TryRemove(key, out var _);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1.0), token);
                } while (!cancel.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                // Expected exception.
            }
        }
        /// <summary>
        /// Registers an asynchronous Rest response as a Rest job.
        /// </summary>
        /// <param name="task">The asynchronous Rest response.</param>
        /// <returns>The RestJob as registered in the store.</returns>
        public RestJob RegisterJob(Task<RestResponse> task)
        {
            var job = new RestJob(identityProvider, task);
            var key = GetKey(job.Id);
            if (!entries.TryAdd(key, new Entry(key, job)))
                throw new ArgumentException("Entry has already been registered.");
            return job;
        }
        /// <summary>
        /// Gets a RestJob from the store.
        /// </summary>
        /// <param name="id">The identity value of the job.</param>
        /// <returns>The RestJob if it is present in the store, null otherwise.</returns>
        public RestJob GetJob(IIdentity<RestJob> id)
            => entries.TryGetValue(GetKey(id), out var entry) ? entry.Job : null;

        private string GetKey(IIdentity<RestJob> id)
            => id.Provider.GetConverter(typeof(RestJob), false).Convert(id.Value).To<string>();

        /// <summary>
        /// Disposes the store.
        /// Cancels the scavenging task.
        /// </summary>
        public void Dispose()
        {
            cancel.Cancel();
        }
        /// <summary>
        /// Gets a job controller from the job store.
        /// </summary>
        /// <param name="id">The identity value for the job controller.</param>
        /// <returns>A RestJobController instance if it was found, null otherwise.</returns>
        public RestJobController GetController(IIdentity<RestJob, RestJobController> id)
        {
            var cid = id as IIdentity<RestJob, RestJobController>;
            var jobId = GetKey(cid?.Parent);
            if (jobId == null)
                return null;
            if (manualEntries.TryGetValue(jobId, out var entry) && entry.Id.Equals(id))
            {
                return new RestJobController(this, cid);
            }
            else
                return null;
        }
        /// <summary>
        /// Creates a RestJob.
        /// </summary>
        /// <returns>A controller for the RestJob.</returns>
        public RestJobController CreateJob()
        {
            var idval = (Guid.NewGuid(), Guid.NewGuid());
            var id = identityProvider.Creator<RestJobController>().Create(idval) as IIdentity<RestJob, RestJobController>;
            var ctrl = new RestJobController(this, id);
            var entry = new ManualEntry(id);
            var key = GetKey(id.Parent);
            if (entries.TryAdd(key, new Entry(key, new RestJob(id.Parent, entry.Response))))
            {
                if (manualEntries.TryAdd(key, entry))
                    return new RestJobController(this, id);
                else
                {
                    entries.TryRemove(key, out var _);
                    return null;
                }
            } else
                return null;
        }
        /// <summary>
        /// Finishes a RestJob corresponding to the controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="value">The result value for the job.</param>
        public void FinishJob(RestJobController controller, object value)
        {
            var key = GetKey(controller.JobId);
            if(manualEntries.TryGetValue(key, out var entry))
            {
                if (entry.Id.Equals(controller.Id))
                    entry.Finish(value);
            }
        }
    }
}
