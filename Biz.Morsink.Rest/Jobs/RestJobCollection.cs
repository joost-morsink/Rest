using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Jobs
{
    /// <summary>
    /// Rest collection implementation for RestJobs.
    /// Needed only for the type ref.
    /// </summary>
    public abstract class RestJobCollection : RestCollection<RestJob>
    {
        /// <summary>
        /// Constructor.
        /// Private: Should never be called.
        /// </summary>
        private RestJobCollection(IIdentity<RestJobCollection> id, IEnumerable<RestJob> items, int count, int? limit, int skip)
            : base(id, items, count, limit, skip)
        { }
    }
}
