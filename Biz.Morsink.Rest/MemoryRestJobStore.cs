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
        private readonly ConcurrentDictionary<string, Entry> entries;
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
                            entries.TryRemove(key, out entry);
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
    }
}
