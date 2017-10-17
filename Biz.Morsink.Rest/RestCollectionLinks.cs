using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class RestCollectionLinks<T,E> : IDynamicLinkProvider<T>
        where T: RestCollection<E>
    {
        public const string first = nameof(first);
        public const string last = nameof(last);
        public const string prev = nameof(prev);
        public const string next = nameof(next);
        public const string skip = nameof(skip);
        public const string limit = nameof(limit);

        public IReadOnlyList<Link> GetLinks(T resource)
        {
            var res = new List<Link>();
            var conv = resource.Id.Provider.GetConverter(typeof(T), false).Convert(resource.Id.Value);
            var dict = conv.To<Dictionary<string, string>>().ToImmutableDictionary();
            var cp = conv.To<CollectionParameters>();
            if (cp.Limit.HasValue)
            {
                res.Add(Link.Create(first, FreeIdentity<T>.Create(
                    dict.SetItem(limit, cp.Limit.Value.ToString())
                    .SetItem(skip, "0")
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
                res.Add(Link.Create(last, FreeIdentity<T>.Create(
                    dict.SetItem(limit, cp.Limit.Value.ToString())
                    .SetItem(skip, ((resource.Count - 1) / cp.Limit.Value * cp.Limit.Value).ToString())
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
                if (cp.Skip > 0)
                    res.Add(Link.Create(prev, FreeIdentity<T>.Create(
                        dict.SetItem(limit, cp.Limit.Value.ToString())
                        .SetItem(skip, Math.Max(0, cp.Skip - cp.Limit.Value).ToString())
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
                if (cp.Skip + cp.Limit.Value < resource.Count)
                    res.Add(Link.Create(next, FreeIdentity<T>.Create(
                        dict.SetItem(limit, cp.Limit.Value.ToString())
                        .SetItem(skip, (cp.Skip + cp.Limit.Value).ToString())
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
            }
            return res;
        }
    }
}
