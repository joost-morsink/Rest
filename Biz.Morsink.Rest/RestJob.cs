using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public class RestJob : IHasIdentity<RestJob>
    {
        public RestJob(IIdentityProvider provider, Task<RestResponse> task)
        {
            Id = provider.Creator<RestJob>().Create(Guid.NewGuid());
            Task = task;
            SetDate();
        }
        private async void SetDate()
        {
            await Task;
            Finished = DateTime.UtcNow;
        }
        public IIdentity<RestJob> Id { get; }
        public Task<RestResponse> Task { get; }
        public DateTime? Finished { get; private set; }

        IIdentity IHasIdentity.Id => Id;
    }
}
