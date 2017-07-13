using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest
{
    public struct RestValue<T> : IRestValue
    {
        public RestValue(T value, IEnumerable<Link> links=null, IEnumerable<object> embeddings=null)
        {
            Value = value;
            Links = (links ?? Enumerable.Empty<Link>()).ToArray();
            Embeddings = (embeddings ?? Enumerable.Empty<object>()).ToArray();
        }
        public T Value { get; }
        public IReadOnlyList<Link> Links { get; }
        public IReadOnlyList<object> Embeddings { get; }
        object IRestValue.Value => Value;
    }
}
