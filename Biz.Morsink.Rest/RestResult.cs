using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Jobs;
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
        public static RestResult<T>.Success Create<T>(T value, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null) 
            => new RestResult<T>.Success(value, links, embeddings);
        /// <summary>
        /// Creates a failed Rest result indicating the request was in some way not correct.
        /// </summary>
        /// <typeparam name="T">The type of the (absent) underlying value.</typeparam>
        /// <param name="data">Data describing the reason the request was bad.</param>
        /// <returns>A failed Rest result indicating the request was in some way not correct.</returns>
        public static RestResult<T>.Failure.BadRequest BadRequest<T>(object data) 
            => new RestResult<T>.Failure.BadRequest(data);
        /// <summary>
        /// Creates a failed Rest result indicating the resource was not found.
        /// </summary>
        /// <typeparam name="T">The type of the (not found) underlying value.</typeparam>
        /// <returns>A failed Rest result indicating the resource was not found.</returns>
        public static RestResult<T>.Failure.NotFound NotFound<T>() 
            => RestResult<T>.Failure.NotFound.Instance;
        /// <summary>
        /// Creates a failed Rest result indicating an unexpected error occurred during processing of the request.
        /// </summary>
        /// <typeparam name="T">The type of the (absent) underlying value.</typeparam>
        /// <param name="ex">An exceptipn describing the error.</param>
        /// <returns>A failed Rest result indicating an error occurred during processing.</returns>
        public static RestResult<T>.Failure.Error Error<T>(Exception ex) 
            => new RestResult<T>.Failure.Error(ExceptionInfo.Create(ex));
        /// <summary>
        /// Creates a pending result indicating the response is not yet available.
        /// </summary>
        /// <typeparam name="T">The type of the (absent) underlying value.</typeparam>
        /// <param name="job">A RestJob describing the pending response.</param>
        /// <returns>A pending Rest result.</returns>
        public static RestResult<T>.Pending Pending<T>(RestJob job) 
            => new RestResult<T>.Pending(job);
        /// <summary>
        /// Creates a failed Rest result indicating an unexpected error occurred during processing of the request.
        /// </summary>
        /// <param name="type">The type of the (absent) underlying value.</param>
        /// <param name="ex">An exceptipn describing the error.</param>
        /// <returns>A failed Rest result indicating an error occurred during processing.</returns>
        public static IRestFailure Error(Type type, Exception ex)
            => (IRestFailure)Activator.CreateInstance(typeof(RestResult<>.Failure.Error).MakeGenericType(type), ExceptionInfo.Create(ex), null, null);
    }
    /// <summary>
    /// This class represents a Rest result.
    /// </summary>
    /// <typeparam name="T">The type of the underlying value for the result.</typeparam>
    public abstract class RestResult<T> : IRestResult
    {
        /// <summary>
        /// Tries to cast the result to a Success result.
        /// </summary>
        /// <returns>The current instance as a Success if it is, null otherwise.</returns>
        public Success AsSuccess() => this as Success;
        IRestSuccess IRestResult.AsSuccess() => this as IRestSuccess;
        /// <summary>
        /// Tries to cast the result to a Failure result.
        /// </summary>
        /// <returns>The current instance as a Failure if it is, null otherwise.</returns>
        public Failure AsFailure() => this as Failure;
        IRestFailure IRestResult.AsFailure() => this as IRestFailure;
        /// <summary>
        /// Tries to cast the result to a Redirect result.
        /// </summary>
        /// <returns>The current instance as a Redirect if it is, null otherwise.</returns>
        public Redirect AsRedirect() => this as Redirect;
        IRestRedirect IRestResult.AsRedirect() => this as IRestRedirect;
        /// <summary>
        /// Tries to cast the result to a Pending result.
        /// </summary>
        /// <returns>The current instance as a Pending if it is, null otherwise.</returns>
        public Pending AsPending() => this as Pending;
        IRestPending IRestResult.AsPending() => this as IRestPending;

        bool IRestResult.IsSuccess => this is Success;
        bool IRestResult.IsFailure => this is Failure;
        bool IRestResult.IsRedirect => this is Redirect;
        bool IRestResult.IsPending => this is Pending;

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
            public Success(IRestValue<T> restValue)
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
            public IRestValue<T> RestValue { get; }

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
                public BadRequest(IRestValue<object> restValue)
                {
                    RestValue = restValue;
                }
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="data">A value describing the reason why the request was bad.</param>
                /// <param name="links">An optional collection of links for the result.</param>
                /// <param name="embeddings">An optional collection of embeddings for the result.</param>
                public BadRequest(object data, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
                    : this(new RestValue<object>(data, links, embeddings))
                { }
                /// <summary>
                /// A Rest value describing the reason why the request was bad.
                /// </summary>
                public IRestValue<object> RestValue { get; }
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
            /// This class represents the request could not be executed.
            /// </summary>
            public class NotExecuted : Failure, IHasRestValue<object>
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="restValue">A Rest value describing the reason why the request was not executed.</param>
                public NotExecuted(RestValue<object> restValue)
                {
                    RestValue = restValue;
                }
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="data">A value describing the reason why the request was not executed.</param>              
                /// <param name="links">An optional collection of links for the result.</param>
                /// <param name="embeddings">An optional collection of embeddings for the result.</param>
                public NotExecuted(object data, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
                    : this(new RestValue<object>(data, links, embeddings))
                { }
                /// <summary>
                /// A Rest value describing the reason why the request was not executed.
                /// </summary>
                public IRestValue<object> RestValue { get; }
                /// <summary>
                /// A value describing the reason why the request was not executed.
                /// </summary>
                public object Data => RestValue.Value;

                IRestValue IHasRestValue.RestValue => RestValue;

                /// <summary>
                /// Changes the underlying successful value type for the Failure.
                /// </summary>
                /// <typeparam name="U">The new underlying successful value type.</typeparam>
                /// <returns>A new NotExecuted failure for type U.</returns>
                public override RestResult<U>.Failure Select<U>()
                    => new RestResult<U>.Failure.NotExecuted(RestValue);
                public override RestFailureReason Reason => RestFailureReason.NotExecuted;

            }
            /// <summary>
            /// This class represents an unexpected error occurred during processing of the request.
            /// </summary>
            public class Error : Failure, IHasRestValue<ExceptionInfo>
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="restValue">A Rest value containing an exception describing the unexpected error.</param>
                public Error(IRestValue<ExceptionInfo> restValue)
                {
                    RestValue = restValue;
                }
                /// <summary>
                /// Constructor/
                /// </summary>
                /// <param name="ex">An exception describing the unexpected error.</param>
                /// <param name="links">An optional collection of links for the result.</param>
                /// <param name="embeddings">An optional collection of embeddings for the result.</param>
                public Error(ExceptionInfo ex, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
                    : this(new RestValue<ExceptionInfo>(ex, links, embeddings))
                { }
                /// <summary>
                /// Gets a Rest value for the exception describing the unexpected error.
                /// </summary>
                public IRestValue<ExceptionInfo> RestValue { get; }
                /// <summary>
                /// Gets the exception describing the unexpected error.
                /// </summary>
                public ExceptionInfo Exception => RestValue.Value;

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
            public abstract RestResult<U>.Failure Select<U>();
            RestResult<U> IRestFailure.Select<U>() => Select<U>();
            /// <summary>
            /// Gets the reason for failure of the Rest request.
            /// </summary>
            public abstract RestFailureReason Reason { get; }
        }
        /// <summary>
        /// This abstract base class represents Rest redirects.
        /// </summary>
        public abstract class Redirect : RestResult<T>, IRestRedirect
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="target">The target of the redirect. (optional for NotNecessary)</param>
            protected Redirect(IIdentity target)
            {
                Target = target;
            }
            /// <summary>
            /// Gets the type of redirect.
            /// </summary>
            public abstract RestRedirectType Type { get; }
            /// <summary>
            /// Gets the target of the redirect.
            /// </summary>
            public IIdentity Target { get; }
            /// <summary>
            /// This abstract method can be used to transform the underlying successful value type into another.
            /// Changing the type is trivial, because redirect results do not contain any actual data of the successful value type.
            /// Therefore an instance of Func&lt;T, U&gt; is not necessary.
            /// </summary>
            /// <typeparam name="U">The new underlying successful value type.</typeparam>
            /// <returns>A new redirect with a different underlying successful value type.</returns
            public abstract RestResult<U> Select<U>();
            /// <summary>
            /// This class represents permanent redirects. 
            /// The target of the redirect may be cached for future references by the client.
            /// </summary>
            public class Permanent : Redirect, IHasRestValue<object>
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="target">The target of the redirect.</param>
                /// <param name="value">An optional underlying Rest value.</param>
                public Permanent(IIdentity target, IRestValue<object> value) : base(target)
                {
                    if (target == null)
                        throw new ArgumentNullException(nameof(target));
                    RestValue = value;
                }
                public override RestRedirectType Type => RestRedirectType.Permanent;

                /// <summary>
                /// Gets an optional underlying Rest value for the redirect.
                /// </summary>
                public IRestValue<object> RestValue { get; }

                IRestValue IHasRestValue.RestValue => RestValue;

                public override RestResult<U> Select<U>()
                    => new RestResult<U>.Redirect.Permanent(Target, RestValue);
            }
            /// <summary>
            /// This class represents temporary redirects.
            /// The target of the redirect can only be used once in this context.
            /// </summary>
            public class Temporary : Redirect, IHasRestValue<object>
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="target">The target of the redirect.</param>
                /// <param name="value">An optional underlying Rest value.</param>
                public Temporary(IIdentity target, IRestValue<object> value) : base(target)
                {
                    if (target == null)
                        throw new ArgumentNullException(nameof(target));
                    RestValue = value;
                }
                public override RestRedirectType Type => RestRedirectType.Temporary;

                /// <summary>
                /// Gets an optional underlying Rest value for the redirect.
                /// </summary>
                public IRestValue<object> RestValue { get; }

                IRestValue IHasRestValue.RestValue => RestValue;

                public override RestResult<U> Select<U>()
                    => new RestResult<U>.Redirect.Temporary(Target, RestValue);
            }
            /// <summary>
            /// This class represents that a response is not necessary and is mathematically a redirect to void.
            /// The client is supposed to already have the information the response would give.
            /// </summary>
            public class NotNecessary : Redirect
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="target">The target of the redirect.</param>
                public NotNecessary(IIdentity target = null) : base(target) { }
                public override RestRedirectType Type => RestRedirectType.NotNecessary;

                public override RestResult<U> Select<U>()
                    => new RestResult<U>.Redirect.NotNecessary(Target);
            }
        }
        /// <summary>
        /// This class represents pending Rest results.
        /// </summary>
        public class Pending : RestResult<T>, IRestPending
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="job">The RestJob describing the pending response.</param>
            public Pending(RestJob job) : base()
            {
                Job = job;
            }
            /// <summary>
            /// Gets a RestJob describing the pending response.
            /// </summary>
            public RestJob Job { get; }

            public RestResult<U> Select<U>()
                => new RestResult<U>.Pending(Job);
        }
        /// <summary>
        /// Implementation of the Linq Select method.
        /// </summary>
        /// <typeparam name="U">The new underlying successful value type.</typeparam>
        /// <param name="f">A manipulation function to manipulate successful Rest values.</param>
        /// <returns>A new RestResult</returns>
        public RestResult<U> Select<U>(Func<IRestValue<T>, IRestValue<U>> f)
        {
            switch (this)
            {
                case Success success:
                    return new RestResult<U>.Success(f(success.RestValue));
                case Failure failure:
                    return failure.Select<U>();
                case Redirect redirect:
                    return redirect.Select<U>();
                case Pending pending:
                    return pending.Select<U>();
                default:
                    throw new NotSupportedException();
            }

        }
        IRestResult IRestResult.Select(Func<IRestValue, IRestValue> f)
            => Select(rv => (IRestValue<T>)f(rv));

        /// <summary>
        /// Makes this result into a redirect result of type NotNecessary.
        /// Used if Version tokens match.
        /// </summary>
        /// <returns>A Redirect.NotNecessary instance.</returns>
        public Redirect.NotNecessary MakeNotNecessary()
            => new Redirect.NotNecessary();
        IRestRedirect IRestResult.MakeNotNecessary()
            => MakeNotNecessary();
    }
}
