using System;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public abstract class RestResponse
    {
        public abstract bool IsSuccess { get; }
        public ValueTask<RestResponse> ToAsync()
            => new ValueTask<RestResponse>(this);

    }
    public class RestResponse<T> : RestResponse
        where T : class
    {
        public RestResponse(RestResult<T> value)
        {
            Value = value;
        }
        public RestResult<T> Value { get; }
        public override bool IsSuccess => Value is IRestSuccess;
    }
}