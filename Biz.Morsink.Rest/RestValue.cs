using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public struct RestValue<T> : IRestValue
        where T:class
    {
        public RestValue(T value, IEnumerable<Link> links = null, IEnumerable<object> embeddings = null)
        {
            Value = value;
            Links = (links ?? Enumerable.Empty<Link>()).ToArray();
            Embeddings = (embeddings ?? Enumerable.Empty<object>()).ToArray();
        }
        public T Value { get; }
        public IReadOnlyList<Link> Links { get; }
        public IReadOnlyList<object> Embeddings { get; }
        object IRestValue.Value => Value;
        public RestResult<T>.Success ToResult()
            => new RestResult<T>.Success(this);
        public ValueTask<RestResult<T>> ToResultAsync()
            => new ValueTask<RestResult<T>>(ToResult());
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
                => new RestResult<T>.Success(Build());
            public ValueTask<RestResult<T>> BuildAsyncResult()
                => new ValueTask<RestResult<T>>(BuildResult());
        }


    }

}
