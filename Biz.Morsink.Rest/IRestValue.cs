using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IRestValue
    {
        object Value { get; }
        IReadOnlyList<Link> Links { get; }
        IReadOnlyList<object> Embeddings { get; }
        IRestValue Manipulate(Func<IRestValue, IEnumerable<Link>> links = null, Func<IRestValue, IEnumerable<object>> embeddings = null);
    }
}
