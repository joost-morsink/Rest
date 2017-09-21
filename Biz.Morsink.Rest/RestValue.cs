using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A structure containing all the necessary components to form a Rest value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct RestValue<T> : IRestValue
        where T : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">An underlying (main) value.</param>
        /// <param name="links">An optional collection of links for the value.</param>
        /// <param name="embeddings">An optional collection of embeddings for the value.</param>
        public RestValue(T value, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
        {
            Value = value;
            Links = links is IReadOnlyList<Link> rolLink ? rolLink : (links ?? Enumerable.Empty<Link>()).ToArray();
            Embeddings = embeddings is IReadOnlyList<object> rolEmbedding ? rolEmbedding : (embeddings ?? Enumerable.Empty<object>()).ToArray();
        }
        /// <summary>
        /// Gets the underlying (main) value.
        /// </summary>
        public T Value { get; }
        /// <summary>
        /// Gets a list of links for the value.
        /// </summary>
        public IReadOnlyList<Link> Links { get; }
        /// <summary>
        /// Gets a list of embeddings for the value.
        /// </summary>
        public IReadOnlyList<object> Embeddings { get; }
        object IRestValue.Value => Value;
        /// <summary>
        /// Implementation of Linq Select method.
        /// </summary>
        /// <typeparam name="U">The new underlying type for the RestValue.</typeparam>
        /// <param name="f">A manipulation function for the underlying (main) value.</param>
        /// <returns>A new RestValue containing the manipulated underlying value and the same links and embeddings.</returns>
        public RestValue<U> Select<U>(Func<T, U> f)
            where U : class
            => new RestValue<U>(f(Value), Links, Embeddings);
        /// <summary>
        /// Creates a new RestValue&lt;T&gt; manipulating the Links and Embeddings collections
        /// </summary>
        /// <param name="links">An optional function to manipulate the Links.</param>
        /// <param name="embeddings">An optional function to manipulate the embeddings.</param>
        /// <returns>A new RestValue with manipualted Links and/or Embeddings.</returns>
        public RestValue<T> Manipulate(Func<RestValue<T>, IEnumerable<Link>> links = null, Func<RestValue<T>, IEnumerable<object>> embeddings = null)
        {
            var l = links == null ? Links : links(this);
            var e = embeddings == null ? Embeddings : embeddings(this);
            return new RestValue<T>(Value, l, e);
        }
        IRestValue IRestValue.Manipulate(Func<IRestValue, IEnumerable<Link>> links, Func<IRestValue, IEnumerable<object>> embeddings)
            => Manipulate(links == null ? (Func<RestValue<T>,IEnumerable<Link>>)null : rv => links(rv), 
                embeddings == null ? (Func<RestValue<T>,IEnumerable<object>>)null : rv => embeddings(rv));
        /// <summary>
        /// Converts the value to a successful RestResult.
        /// </summary>
        /// <returns>A successful RestResult.</returns>
        public RestResult<T>.Success ToResult()
            => new RestResult<T>.Success(this);
        /// <summary>
        /// Converts the value to a successful RestResponse.
        /// </summary>
        /// <param name="metadata">Optional metadata collection for the response.</param>
        /// <returns>A successful RestResponse.</returns>
        public RestResponse<T> ToResponse(TypeKeyedDictionary metadata = null)
            => ToResult().ToResponse(metadata);
        /// <summary>
        /// Converts the value to a ValueTask containing a successful RestResponse.
        /// </summary>
        /// <param name="metadata">Optional metadata collection for the response.</param>
        /// <returns>A successful RestResponse wrapped in a ValueTask.</returns>
        public ValueTask<RestResponse<T>> ToResponseAsync(TypeKeyedDictionary metadata = null)
            => ToResponse(metadata).ToAsync();
        /// <summary>
        /// Create a Builder for a RestValue&lt;T&gt;
        /// </summary>
        /// <returns></returns>
        public static Builder Build()
            => new Builder(default(T), ImmutableList<Link>.Empty, ImmutableList<object>.Empty);
        /// <summary>
        /// Builder struct for RestValue&lt;T&gt;
        /// </summary>
        public struct Builder
        {
            private readonly T value;
            private readonly ImmutableList<Link> links;
            private readonly ImmutableList<object> embeddings;

            internal Builder(T value, ImmutableList<Link> links, ImmutableList<object> embeddings)
            {
                this.value = value;
                this.links = links;
                this.embeddings = embeddings;
            }
            /// <summary>
            /// Creates a Builder with a different Value.
            /// </summary>
            /// <param name="value">The new Value property.</param>
            /// <returns>A Builder with a different Value.</returns>
            public Builder WithValue(T value)
                => new Builder(value, links, embeddings);
            /// <summary>
            /// Creates a Builder with an added Link.
            /// </summary>
            /// <param name="link">The Link to add.</param>
            /// <returns>A Builder with an added Link.</returns>
            public Builder WithLink(Link link)
                => new Builder(value, links.Add(link), embeddings);
            /// <summary>
            /// Creates a Builder with added Links.
            /// </summary>
            /// <param name="link">The Links to add.</param>
            /// <returns>A Builder with added Links.</returns>
            public Builder WithLinks(IEnumerable<Link> links)
                => new Builder(value, this.links.AddRange(links), embeddings);
            /// <summary>
            /// Creates a Builder with an added embedding.
            /// </summary>
            /// <param name="embedding">The embedding to add.</param>
            /// <returns>A Builder with an added embedding.</returns>
            public Builder WithEmbedding(object embedding)
                => new Builder(value, links, embeddings.Add(embedding));
            /// <summary>
            /// Creates a Builder with added embeddings.
            /// </summary>
            /// <param name="embedding">The embeddings to add.</param>
            /// <returns>A Builder with added embeddings.</returns>
            public Builder WithEmbeddings(IEnumerable<object> embeddings)
                => new Builder(value, links, this.embeddings.AddRange(embeddings));
            /// <summary>
            /// Build the RestValue&lt;T&gt;
            /// </summary>
            /// <returns>A RestValue.</returns>
            public RestValue<T> Build()
                => new RestValue<T>(value, links, embeddings);
            /// <summary>
            /// Builds a successful RestResult.
            /// </summary>
            /// <returns>A successful RestResult.</returns>
            public RestResult<T>.Success BuildResult()
                => Build().ToResult();
            /// <summary>
            /// Builds a successful RestResult wrapped in a ValueTask.
            /// </summary>
            /// <returns>A successful RestResult wrapped in a ValueTask.</returns>
            public ValueTask<RestResult<T>> BuildAsyncResult()
                => Build().ToResult().ToAsync();
            /// <summary>
            /// Builds a successful RestResponse.
            /// </summary>
            /// <param name="metadata">An optional collection of metadata for the response.</param>
            /// <returns>A successful RestResponse.</returns>
            public RestResponse<T> BuildResponse(TypeKeyedDictionary metadata = null)
                => Build().ToResponse(metadata);
            /// <summary>
            /// Builds  a successful RestResponse wrapped in a ValueTask.
            /// </summary>
            /// <param name="metadata">An optional collection of metadata for the response.</param>
            /// <returns>A successful RestResponse wrapped in a ValueTask.</returns>
            public ValueTask<RestResponse<T>> BuildResponseAsync(TypeKeyedDictionary metadata = null)
                => Build().ToResponseAsync(metadata);
        }
    }
}
