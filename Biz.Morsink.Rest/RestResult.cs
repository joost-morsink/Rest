using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public static class RestResult
    {
        public static RestResult<T>.Success Create<T>(T value, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null) where T : class
            => new RestResult<T>.Success(value, links, embeddings);
        public static RestResult<T>.Failure.BadRequest BadRequest<T>(object data) where T : class
            => new RestResult<T>.Failure.BadRequest(data);
        public static RestResult<T>.Failure.NotFound NotFound<T>() where T : class
            => RestResult<T>.Failure.NotFound.Instance;
        public static RestResult<T>.Failure.Error Error<T>(Exception ex) where T : class
            => new RestResult<T>.Failure.Error(ex);
    }
    public abstract class RestResult<T>
        where T : class
    {
        public Success AsSuccess() => this as Success;
        public Failure AsFailure() => this as Failure;
        
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
            }
            public class NotFound : Failure
            {
                public static NotFound Instance { get; } = new NotFound();
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
            }
        }
    }
}
