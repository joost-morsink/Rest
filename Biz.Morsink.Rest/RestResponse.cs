using System;

namespace Biz.Morsink.Rest
{
    public abstract class RestResponse
    {
        public abstract bool IsSuccess { get; }
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