using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Helper class for Rest results.
    /// </summary>
    public class RestResult
    {
        /// <summary>
        /// Creates a successful Rest result.
        /// </summary>
        /// <typeparam name="T">The type of the underlying value.</typeparam>
        /// <param name="value">The underlying value.</param>
        /// <param name="links">Optionally a collection of links for the result.</param>
        /// <param name="embeddings">Optionally a collection of embeddings for the result.</param>
        /// <returns>A successful Rest result.</returns>
        public static RestResult<T>.Success Create<T>(T value, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null) where T : class
            => new RestResult<T>.Success(value, links, embeddings);
        /// <summary>
        /// Creates a failed Rest result indicating the request was in some way not correct.
        /// </summary>
        /// <typeparam name="T">The type of the (absent) underlying value.</typeparam>
        /// <param name="data">Data describing the reason the request was bad.</param>
        /// <returns>A failed Rest result indicating the request was in some way not correct.</returns>
        public static RestResult<T>.Failure.BadRequest BadRequest<T>(object data) where T : class
            => new RestResult<T>.Failure.BadRequest(data);
        /// <summary>
        /// Creates a failed Rest result indicating the resource was not found.
        /// </summary>
        /// <typeparam name="T">The type of the (not found) underlying value.</typeparam>
        /// <returns>A failed Rest result indicating the resource was not found.</returns>
        public static RestResult<T>.Failure.NotFound NotFound<T>() where T : class
            => RestResult<T>.Failure.NotFound.Instance;
        /// <summary>
        /// Creates a failed Rest result indicating an unexpected error occurred during processing of the request.
        /// </summary>
        /// <typeparam name="T">The type of the (absent) underlying value.</typeparam>
        /// <param name="ex">An exceptipn describing the error.</param>
        /// <returns>A failed Rest result indicating an error occurred during processing.</returns>
        public static RestResult<T>.Failure.Error Error<T>(Exception ex) where T : class
            => new RestResult<T>.Failure.Error(ex);
        /// <summary>
        /// Creates a failed Rest result indicating an unexpected error occurred during processing of the request.
        /// </summary>
        /// <param name="type">The type of the (absent) underlying value.</param>
        /// <param name="ex">An exceptipn describing the error.</param>
        /// <returns>A failed Rest result indicating an error occurred during processing.</returns>
        public static IRestFailure Error(Type type, Exception ex)
            => (IRestFailure)Activator.CreateInstance(typeof(RestResult<>.Failure.Error).MakeGenericType(type), ex, null, null);
    }
    /// <summary>
    /// This class represents a Rest result.
    /// </summary>
    /// <typeparam name="T">The type of the underlying value for the result.</typeparam>
    public abstract class RestResult<T> : IRestResult
        where T : class
    {
        /// <summary>
        /// Tries to cast the result to a Success result.
        /// </summary>
        /// <returns>The current instance as a Success if it is, null otherwise.</returns>
        public Success AsSuccess() => this as Success;
        IRestSuccess IRestResult.AsSuccess() => this as IRestSuccess;
        /// <summary>
        /// Triews to cast the result to a Failure result.
        /// </summary>
        /// <returns>The current instance as a Failure if it is, null otherwise.</returns>
        public Failure AsFailure() => this as Failure;
        IRestFailure IRestResult.AsFailure() => this as IRestFailure;


        public Redirect AsRedirect() => this as Redirect;
        IRestRedirect IRestResult.AsRedirect() => this as IRestRedirect;

        bool IRestResult.IsSuccess => this is Success;
        bool IRestResult.IsFailure => this is Failure;
        bool IRestResult.IsRedirect => this is Redirect;
        /// <summary>
        /// Wraps this result into a RestResponse, optionally adding metadata.
        /// </summary>
        /// <param name="metadata">Metadata for the Rest response.</param>
        /// <returns>A RestResponse wrapping the result.</returns>
        public RestResponse<T> ToResponse(TypeKeyedDictionary metadata = null) => new RestResponse<T>(this, metadata);
        RestResponse IRestResult.ToResponse(TypeKeyedDictionary metadata) => ToResponse(metadata);
        /// <summary>
        /// Wraps this result into a RestResponse and into a ValueTask, optionally adding metadata.
        /// </summary>
        /// <param name="metadata">Metadata for the Rest response.</param>
        /// <returns>A ValueTask containing a RestResponse wrapping the result.</returns>
        public ValueTask<RestResponse<T>> ToResponseAsync(TypeKeyedDictionary metadata = null) => ToResponse(metadata).ToAsync();
        /// <summary>
        /// Wraps this result into a ValueTask.
        /// </summary>
        /// <returns>A ValueTask wrapping the result.</returns>
        public ValueTask<RestResult<T>> ToAsync() => new ValueTask<RestResult<T>>(this);
        /// <summary>
        /// This class represents successful Rest results.
        /// </summary>
        public class Success : RestResult<T>, IRestSuccess<T>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="restValue">The underlying Rest value for the successful result.</param>
            public Success(RestValue<T> restValue)
            {
                RestValue = restValue;
            }
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="value">The underlying value for the successful result.</param>
            /// <param name="links">An optional collection of links for the result.</param>
            /// <param name="embeddings">An optional collection of embeddings for the result.</param>
            public Success(T value, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
               : this(new RestValue<T>(value, links, embeddings))
            { }
            /// <summary>
            /// Gets the underlying value.
            /// </summary>
            public T Value => RestValue.Value;
            /// <summary>
            /// Gets a collection of links for the result.
            /// </summary>
            public IReadOnlyCollection<Link> Links => RestValue.Links;
            /// <summary>
            /// Gets a collection of embeddings for the result.
            /// </summary>
            public IReadOnlyCollection<object> Embeddings => RestValue.Embeddings;
            /// <summary>
            /// Gets the underlying RestValue.
            /// </summary>
            public RestValue<T> RestValue { get; }

            IRestValue IHasRestValue.RestValue => RestValue;
        }
        /// <summary>
        /// This abstract base class represents failed Rest results.
        /// </summary>
        public abstract class Failure : RestResult<T>, IRestFailure
        {
            /// <summary>
            /// This class represents responses to bad requests.
            /// </summary>
            public class BadRequest : Failure, IHasRestValue<object>
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="restValue">A Rest value describing the reason why the request was bad.</param>
                public BadRequest(RestValue<object> restValue)
                {
                    RestValue = restValue;
                }
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="restValue">A value describing the reason why the request was bad.</param>
                /// <param name="links">An optional collection of links for the result.</param>
                /// <param name="embeddings">An optional collection of embeddings for the result.</param>
                public BadRequest(object data, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
                    : this(new RestValue<object>(data, links, embeddings))
                { }
                /// <summary>
                /// A Rest value describing the reason why the request was bad.
                /// </summary>
                public RestValue<object> RestValue { get; }
                /// <summary>
                /// A value describing the reason why the request was bad.
                /// </summary>
                public object Data => RestValue.Value;

                IRestValue IHasRestValue.RestValue => RestValue;

                /// <summary>
                /// Changes the underlying successful value type for the Failure.
                /// </summary>
                /// <typeparam name="U">The new underlying successful value type.</typeparam>
                /// <returns>A new BadRequest failure for type U.</returns>
                public override RestResult<U>.Failure Select<U>()
                    => new RestResult<U>.Failure.BadRequest(RestValue);
                public override RestFailureReason Reason => RestFailureReason.BadRequest;
            }
            /// <summary>
            /// This class represents the resource addressed in the request could not be found.
            /// </summary>
            public class NotFound : Failure
            {
                /// <summary>
                /// Gets an instance of the NotFound class.
                /// </summary>
                public static NotFound Instance { get; } = new NotFound();
                /// <summary>
                /// Changes the underlying successful value type for the Failure.
                /// </summary>
                /// <typeparam name="U">The new underlying successful value type.</typeparam>
                /// <returns>A new NotFound failure for type U.</returns>
                public override RestResult<U>.Failure Select<U>()
                    => RestResult<U>.Failure.NotFound.Instance;
                public override RestFailureReason Reason => RestFailureReason.NotFound;

            }
            /// <summary>
            /// This class represents an unexpected error occurred during processing of the request.
            /// </summary>
            public class Error : Failure, IHasRestValue<Exception>
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="restValue">A Rest value containing an exception describing the unexpected error.</param>
                public Error(RestValue<Exception> restValue)
                {
                    RestValue = restValue;
                }
                /// <summary>
                /// Constructor/
                /// </summary>
                /// <param name="ex">An exception describing the unexpected error.</param>
                /// <param name="links">An optional collection of links for the result.</param>
                /// <param name="embeddings">An optional collection of embeddings for the result.</param>
                public Error(Exception ex, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
                    : this(new RestValue<Exception>(ex, links, embeddings))
                { }
                /// <summary>
                /// Gets a Rest value for the exception describing the unexpected error.
                /// </summary>
                public RestValue<Exception> RestValue { get; }
                /// <summary>
                /// Gets the exception describing the unexpected error.
                /// </summary>
                public Exception Exception => RestValue.Value;

                IRestValue IHasRestValue.RestValue => RestValue;
                /// <summary>
                /// Changes the underlying successful value type for the Failure.
                /// </summary>
                /// <typeparam name="U">The new underlying successful value type.</typeparam>
                /// <returns>A new Error failure for type U.</returns>
                public override RestResult<U>.Failure Select<U>()
                    => new RestResult<U>.Failure.Error(RestValue);
                public override RestFailureReason Reason => RestFailureReason.Error;

            }
            /// <summary>
            /// This abstract method can be used to transform the underlying successful value type into another.
            /// Changing the type is trivial, because failure results do not contain any actual data of the successful value type.
            /// Therefore an instance of Func&lt;T, U&gt; is not necessary.
            /// </summary>
            /// <typeparam name="U">The new underlying successful value type.</typeparam>
            /// <returns>A new failure with a different underlying successful value type.</returns>
            public abstract RestResult<U>.Failure Select<U>()
                where U : class;
            /// <summary>
            /// Gets the reason for failure of the Rest request.
            /// </summary>
            public abstract RestFailureReason Reason { get; }
        }
        public abstract class Redirect : RestResult<T>, IRestRedirect
        {
            protected Redirect(IIdentity target)
            {
                Target = target;
            }
            public abstract RestRedirectType Type { get; }
            public IIdentity Target { get; }

            public abstract RestResult<U> Select<U>() where U : class;



            public class Permanent : Redirect, IHasRestValue<object>
            {
                public Permanent(IIdentity target, RestValue<object> value) : base(target)
                {
                    RestValue = value;
                }
                public override RestRedirectType Type => RestRedirectType.Permanent;

                public RestValue<object> RestValue { get; }

                IRestValue IHasRestValue.RestValue => RestValue;

                public override RestResult<U> Select<U>()
                    => new RestResult<U>.Redirect.Permanent(Target, RestValue);
            }
            public class Temporary : Redirect, IHasRestValue<object>
            {
                public Temporary(IIdentity target, RestValue<object> value) : base(target)
                {
                    RestValue = value;
                }
                public override RestRedirectType Type => RestRedirectType.Temporary;

                public RestValue<object> RestValue { get; }

                IRestValue IHasRestValue.RestValue => RestValue;

                public override RestResult<U> Select<U>()
                    => new RestResult<U>.Redirect.Temporary(Target, RestValue);
            }
            public class NotNecessary : Redirect
            {
                public NotNecessary(IIdentity target = null) : base(target) { }
                public override RestRedirectType Type => RestRedirectType.NotNecessary;

                public override RestResult<U> Select<U>()
                    => new RestResult<U>.Redirect.NotNecessary(Target);
            }
        }
        /// <summary>
        /// Implementation of the Linq Select method.
        /// </summary>
        /// <typeparam name="U">The new underlying successful value type.</typeparam>
        /// <param name="f">A manipulation function to manipulate successful Rest values.</param>
        /// <returns>A new RestResult</returns>
        public RestResult<U> Select<U>(Func<RestValue<T>, RestValue<U>> f)
            where U : class
        {
            switch (this)
            {
                case Success success:
                    return new RestResult<U>.Success(f(success.RestValue));
                case Failure failure:
                    return failure.Select<U>();
                case Redirect redirect:
                    return redirect.Select<U>();
                default:
                    throw new NotSupportedException();
            }

        }
        IRestResult IRestResult.Select(Func<IRestValue, IRestValue> f)
            => Select(rv => (RestValue<T>)f(rv));
    }
}
