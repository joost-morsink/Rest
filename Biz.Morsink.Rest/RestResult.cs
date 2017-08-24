using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public class RestResult
    {
        public static RestResult<T>.Success Create<T>(T value, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null) where T : class
            => new RestResult<T>.Success(value, links, embeddings);
        public static RestResult<T>.Failure.BadRequest BadRequest<T>(object data) where T : class
            => new RestResult<T>.Failure.BadRequest(data);
        public static RestResult<T>.Failure.NotFound NotFound<T>() where T : class
            => RestResult<T>.Failure.NotFound.Instance;
        public static RestResult<T>.Failure.Error Error<T>(Exception ex) where T : class
            => new RestResult<T>.Failure.Error(ex);
        public static IRestFailure Error(Type type, Exception ex)
            => (IRestFailure)Activator.CreateInstance(typeof(RestResult<>.Failure.Error).MakeGenericType(type), ex, null, null);

    }
    public abstract class RestResult<T> : IRestResult
        where T : class
    {
        public Success AsSuccess() => this as Success;
        IRestSuccess IRestResult.AsSuccess() => this as IRestSuccess;
        public Failure AsFailure() => this as Failure;
        IRestFailure IRestResult.AsFailure() => this as IRestFailure;
        bool IRestResult.IsSuccess => this is Success;

        public RestResponse<T> ToResponse(TypeKeyedDictionary metadata = null) => new RestResponse<T>(this, metadata);
        RestResponse IRestResult.ToResponse(TypeKeyedDictionary metadata) => ToResponse(metadata);
        public ValueTask<RestResponse<T>> ToResponseAsync(TypeKeyedDictionary metadata = null) => ToResponse(metadata).ToAsync();
        public ValueTask<RestResult<T>> ToAsync() => new ValueTask<RestResult<T>>(this);

        public class Success : RestResult<T>, IRestSuccess<T>
        {
            public Success(RestValue<T> restValue)
            {
                RestValue = restValue;
            }
            public Success(T value, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
               : this(new RestValue<T>(value, links, embeddings))
            { }
            public T Value => RestValue.Value;
            public IReadOnlyCollection<Link> Links => RestValue.Links;
            public IReadOnlyCollection<object> Embeddings => RestValue.Embeddings;
            public RestValue<T> RestValue { get; }

            IRestValue IHasRestValue.RestValue => RestValue;
        }
        public abstract class Failure : RestResult<T>, IRestFailure
        {
            public class BadRequest : Failure, IHasRestValue<object>
            {
                public BadRequest(RestValue<object> restValue)
                {
                    RestValue = restValue;
                }
                public BadRequest(object data, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
                    : this(new RestValue<object>(data, links, embeddings))
                { }
                public RestValue<object> RestValue { get; }
                public object Data => RestValue.Value;

                IRestValue IHasRestValue.RestValue => RestValue;

                public override RestResult<U>.Failure Select<U>()
                    => new RestResult<U>.Failure.BadRequest(RestValue);
            }
            public class NotFound : Failure
            {
                public static NotFound Instance { get; } = new NotFound();
                public override RestResult<U>.Failure Select<U>()
                    => RestResult<U>.Failure.NotFound.Instance;
            }
            public class Error : Failure, IHasRestValue<Exception>
            {
                public Error(RestValue<Exception> restValue)
                {
                    RestValue = restValue;
                }
                public Error(Exception ex, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
                    : this(new RestValue<Exception>(ex, links, embeddings))
                { }
                public RestValue<Exception> RestValue { get; }
                public Exception Exception => RestValue.Value;

                IRestValue IHasRestValue.RestValue => RestValue;

                public override RestResult<U>.Failure Select<U>()
                    => new RestResult<U>.Failure.Error(RestValue);
            }
            public abstract RestResult<U>.Failure Select<U>()
                where U : class;
        }

        public RestResult<U> Select<U>(Func<RestValue<T>, RestValue<U>> f)
            where U : class
        {
            switch (this)
            {
                case Success success:
                    return new RestResult<U>.Success(f(success.RestValue));
                case Failure failure:
                    return failure.Select<U>();
                default:
                    throw new NotSupportedException();
            }

        }
        IRestResult IRestResult.Select(Func<IRestValue, IRestValue> f)
            => Select(rv => (RestValue<T>)f(rv));
    }
}
