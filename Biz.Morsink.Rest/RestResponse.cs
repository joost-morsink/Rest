using Biz.Morsink.Rest.Utils;
using System;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public abstract class RestResponse
    {
        protected RestResponse(TypeKeyedDictionary metadata)
        {
            Metadata = metadata ?? TypeKeyedDictionary.Empty;
        }
        public abstract bool IsSuccess { get; }
        public TypeKeyedDictionary Metadata { get; }

        public ValueTask<RestResponse> ToAsync()
            => new ValueTask<RestResponse>(this);
        public abstract RestResponse Select(Func<IRestResult, IRestResult> f);
        public abstract RestResponse AddMetadata<X>(X item);
    }
    public class RestResponse<T> : RestResponse
        where T : class
    {
        public RestResponse(RestResult<T> value, TypeKeyedDictionary metadata) : base(metadata)
        {
            Value = value;
        }
        public RestResult<T> Value { get; }
        public override bool IsSuccess => Value is IRestSuccess;
        public RestResponse<U> Select<U>(Func<RestResult<T>, RestResult<U>> f)
            where U : class
        {
            return new RestResponse<U>(f(Value), Metadata);
        }
        public override RestResponse Select(Func<IRestResult, IRestResult> f)
            => new RestResponse<T>((RestResult<T>)f(Value), Metadata);
        public override RestResponse AddMetadata<X>(X item)
            => new RestResponse<T>(Value, Metadata.Add(item));

        public new ValueTask<RestResponse<T>> ToAsync() => new ValueTask<RestResponse<T>>(this);
    }
}