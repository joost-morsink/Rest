using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IRestResult
    {
        RestResponse ToResponse(RestParameterCollection metadata);
        IRestResult Select(Func<IRestValue, IRestValue> f);
        bool IsSuccess { get; }
        IRestSuccess AsSuccess();
        IRestFailure AsFailure();
    }
}
