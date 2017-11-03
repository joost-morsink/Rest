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
        private readonly TimeSpan cleanupSpan;
        private readonly IIdentityProvider identityProvider;

        public MemoryRestJobStore(IIdentityProvider identityProvider)
        {
            entries = new ConcurrentDictionary<string, Entry>();
            cancel = new CancellationTokenSource();
            cleanupSpan = TimeSpan.FromHours(1.0);
            this.identityProvider = identityProvider;
            scavengeTask = StartScavenging(cancel.Token);
        }

        private async Task StartScavenging(CancellationToken token)
        {
            do
            {
                var now = DateTime.UtcNow;
                var keys = entries.Keys.ToArray();
                foreach (var key in keys)
                {
                    if (entries.TryGetValue(key, out var entry) && entry.Job.Finished.HasValue && now - entry.Job.Finished.Value > cleanupSpan)
                        entries.TryRemove(key, out entry);
                }

                await Task.Delay(TimeSpan.FromMinutes(1.0), token);
            } while (!cancel.IsCancellationRequested);
        }

        public RestJob RegisterJob(Task<RestResponse> task)
        {
            var job = new RestJob(identityProvider, task);
            var key = GetKey(job.Id);
            if (!entries.TryAdd(key, new Entry(key, job)))
                throw new ArgumentException("Entry has already been registered.");
            return job;
        }

        public RestJob GetJob(IIdentity<RestJob> id)
            => entries.TryGetValue(GetKey(id), out var entry) ? entry.Job : null;

        private string GetKey(IIdentity<RestJob> id)
            => id.Provider.GetConverter(typeof(RestJob), false).Convert(id.Value).To<string>();
        
        public void Dispose()
        {
            cancel.Cancel();
        }
    }
}
