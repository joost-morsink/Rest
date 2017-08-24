using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IRestResult
    {
        RestResponse ToResponse(TypeKeyedDictionary metadata = null);
        IRestResult Select(Func<IRestValue, IRestValue> f);
        bool IsSuccess { get; }
        IRestSuccess AsSuccess();
        IRestFailure AsFailure();
    }
}
