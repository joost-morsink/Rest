using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class RestJobResult
    {
        private readonly RestJob restJob;

        public RestJobResult(RestJob job)
        {
            restJob = job ?? throw new ArgumentNullException(nameof(job));
        }

        public IIdentity<RestJob> JobId => restJob.Id;
        public RestJob Job => restJob;
    }
}
