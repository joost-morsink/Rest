using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class RestJobFinished : IHasIdentity<RestJobFinished>
    {
        public RestJobFinished()
        {
        }

        public IIdentity<RestJob, RestJobFinished> Id { get; set; }
        public object Value { get; set; }
        public IIdentity<RestJob, RestJobController> GetControllerId() => Id.Provider.Creator<RestJobController>().Create(Id.Value) as IIdentity<RestJob, RestJobController>;

        IIdentity<RestJobFinished> IHasIdentity<RestJobFinished>.Id => Id;

        IIdentity IHasIdentity.Id => Id;
    }
}
