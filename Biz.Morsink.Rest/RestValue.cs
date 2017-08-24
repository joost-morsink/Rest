using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public struct RestValue<T> : IRestValue
        where T : class
    {
        public RestValue(T value, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
        {
            Value = value;
            Links = links is IReadOnlyList<Link> rolLink ? rolLink : (links ?? Enumerable.Empty<Link>()).ToArray();
            Embeddings = embeddings is IReadOnlyList<object> rolEmbedding ? rolEmbedding : (embeddings ?? Enumerable.Empty<object>()).ToArray();
        }
        public T Value { get; }
        public IReadOnlyList<Link> Links { get; }
        public IReadOnlyList<object> Embeddings { get; }
        object IRestValue.Value => Value;
        public RestValue<U> Select<U>(Func<T, U> f)
            where U : class
            => new RestValue<U>(f(Value), Links, Embeddings);
        public RestValue<T> Manipulate(Func<RestValue<T>, IEnumerable<Link>> links = null, Func<RestValue<T>, IEnumerable<object>> embeddings = null)
        {
            var l = links == null ? Links : links(this);
            var e = embeddings == null ? Embeddings : embeddings(this);
            return new RestValue<T>(Value, l, e);
        }
        IRestValue IRestValue.Manipulate(Func<IRestValue, IEnumerable<Link>> links, Func<IRestValue, IEnumerable<object>> embeddings)
            => Manipulate(links == null ? (Func<RestValue<T>,IEnumerable<Link>>)null : rv => links(rv), 
                embeddings == null ? (Func<RestValue<T>,IEnumerable<object>>)null : rv => embeddings(rv));
        public RestResult<T>.Success ToResult()
            => new RestResult<T>.Success(this);
        public RestResponse<T> ToResponse(TypeKeyedDictionary metadata = null)
            => ToResult().ToResponse(metadata);
        public ValueTask<RestResponse<T>> ToResponseAsync(TypeKeyedDictionary metadata = null)
            => ToResponse(metadata).ToAsync();
        public static Builder Build()
            => new Builder(default(T), ImmutableList<Link>.Empty, ImmutableList<object>.Empty);
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
            public Builder WithValue(T value)
                => new Builder(value, links, embeddings);
            public Builder WithLink(Link link)
                => new Builder(value, links.Add(link), embeddings);
            public Builder WithLinks(IEnumerable<Link> links)
                => new Builder(value, this.links.AddRange(links), embeddings);
            public Builder WithEmbedding(object embedding)
                => new Builder(value, links, embeddings.Add(embedding));
            public Builder WithEmbeddings(IEnumerable<object> embeddings)
                => new Builder(value, links, this.embeddings.AddRange(embeddings));

            public RestValue<T> Build()
                => new RestValue<T>(value, links, embeddings);
            public RestResult<T>.Success BuildResult()
                => Build().ToResult();
            public ValueTask<RestResult<T>> BuildAsyncResult()
                => Build().ToResult().ToAsync();
            public RestResponse<T> BuildResponse(TypeKeyedDictionary metadata = null)
                => Build().ToResponse(metadata);
            public ValueTask<RestResponse<T>> BuildResponseAsync(TypeKeyedDictionary metadata = null)
                => Build().ToResponseAsync(metadata);
        }
    }
}
