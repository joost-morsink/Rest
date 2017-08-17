using System;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public abstract class RestResponse
    {
        public abstract bool IsSuccess { get; }
        public ValueTask<RestResponse> ToAsync()
            => new ValueTask<RestResponse>(this);
        public abstract RestResponse Select(Func<IRestResult, IRestResult> f);

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
        public RestResponse<U> Select<U>(Func<RestResult<T>, RestResult<U>> f)
            where U : class
        {
            return new RestResponse<U>(f(Value));
        }
        public override RestResponse Select(Func<IRestResult, IRestResult> f)
            => new RestResponse<T>((RestResult<T>)f(Value));
    }
}