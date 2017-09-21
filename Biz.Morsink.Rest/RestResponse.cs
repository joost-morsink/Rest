﻿using Biz.Morsink.Rest.Utils;
using System;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Abstract base class for Rest responses.
    /// </summary>
    public abstract class RestResponse
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="metadata">Metadata collection.</param>
        protected RestResponse(TypeKeyedDictionary metadata)
        {
            Metadata = metadata ?? TypeKeyedDictionary.Empty;
        }
        /// <summary>
        /// True if the Rest response represents a successful one.
        /// </summary>
        public abstract bool IsSuccess { get; }
        /// <summary>
        /// Gets the metadata for the response.
        /// </summary>
        public TypeKeyedDictionary Metadata { get; }

        /// <summary>
        /// Wraps the Rest response in a ValueTask.
        /// </summary>
        /// <returns>A ValueTask containing this response.</returns>
        public ValueTask<RestResponse> ToAsync()
            => new ValueTask<RestResponse>(this);
        /// <summary>
        /// Implementation of LinQ Select method.
        /// </summary>
        /// <param name="f">Manipulation function for the wrapped result.</param>
        /// <returns>A new RestResponse.</returns>
        public abstract RestResponse Select(Func<IRestResult, IRestResult> f);
        /// <summary>
        /// Creates a new RestResponse with added metadata.
        /// </summary>
        /// <typeparam name="X">The type of metadata to include in the response.</typeparam>
        /// <param name="item">An instance of metadata to include in the response.</param>
        /// <returns>A new RestResponse with added metadata.</returns>
        public abstract RestResponse AddMetadata<X>(X item);
    }
    /// <summary>
    /// Generic class representing Rest responses.
    /// </summary>
    /// <typeparam name="T">The body type of the response.</typeparam>
    public class RestResponse<T> : RestResponse
        where T : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">A typed RestResult for the response.</param>
        /// <param name="metadata">Metadata for the response.</param>
        public RestResponse(RestResult<T> value, TypeKeyedDictionary metadata) : base(metadata)
        {
            Value = value;
        }
        /// <summary>
        /// Gets the typed Rest result.
        /// </summary>
        public RestResult<T> Value { get; }
        /// <summary>
        /// True if the Value is a successful one.
        /// </summary>
        public override bool IsSuccess => Value is IRestSuccess;
        public RestResponse<U> Select<U>(Func<RestResult<T>, RestResult<U>> f)
            where U : class
        {
            return new RestResponse<U>(f(Value), Metadata);
        }
        /// <summary>
        /// Implementation of the Linq Select method.
        /// </summary>
        /// <param name="f">Manipulation of the inner Rest result value. The resulting value should have the same underlying type.</param>
        /// <returns>A new RestResponse with a manipulateds Rest result.</returns>
        public override RestResponse Select(Func<IRestResult, IRestResult> f)
            => new RestResponse<T>((RestResult<T>)f(Value), Metadata);
        /// <summary>
        /// Creates a new RestResponse with added metadata.
        /// </summary>
        /// <typeparam name="X">The type of metadata to include in the response.</typeparam>
        /// <param name="item">An instance of metadata to include in the response.</param>
        /// <returns>A new RestResponse with added metadata.</returns>
        public override RestResponse AddMetadata<X>(X item)
            => new RestResponse<T>(Value, Metadata.Add(item));
        /// <summary>
        /// Wraps the response in a ValueTask,.
        /// </summary>
        /// <returns>The response wrapped in a ValueTask.</returns>
        public new ValueTask<RestResponse<T>> ToAsync() => new ValueTask<RestResponse<T>>(this);
    }
}