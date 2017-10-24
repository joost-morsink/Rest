using Biz.Morsink.DataConvert;
using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class PersonCollectionLinks : IDynamicLinkProvider<PersonCollection>
    {
        public IReadOnlyList<Link> GetLinks(PersonCollection resource)
        {
            var res = new List<Link>();
            var conv = resource.Id.Provider.GetConverter(typeof(PersonCollection), false).Convert(resource.Id.Value);
            var dict = conv.To<Dictionary<string, string>>().ToImmutableDictionary();
            var ssp = conv.To<SimpleSearchParameters>();
            var cp = conv.To<CollectionParameters>();
            if (cp.Limit.HasValue)
            {
                res.Add(Link.Create("first", FreeIdentity<PersonCollection>.Create(
                    dict.SetItem("limit", cp.Limit.Value.ToString())
                    .SetItem("skip", "0")
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
                res.Add(Link.Create("last", FreeIdentity<PersonCollection>.Create(
                    dict.SetItem("limit", cp.Limit.Value.ToString())
                    .SetItem("skip", ((resource.Count - 1) / cp.Limit.Value * cp.Limit.Value).ToString())
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
                if (cp.Skip > 0)
                    res.Add(Link.Create("prev", FreeIdentity<PersonCollection>.Create(
                        dict.SetItem("limit", cp.Limit.Value.ToString())
                        .SetItem("skip", Math.Max(0, cp.Skip - cp.Limit.Value).ToString())
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
                if (cp.Skip + cp.Limit.Value < resource.Count)
                    res.Add(Link.Create("next", FreeIdentity<PersonCollection>.Create(
                        dict.SetItem("limit", cp.Limit.Value.ToString())
                        .SetItem("skip", (cp.Skip + cp.Limit.Value).ToString())
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))));
            }
            return res;
        }
    }
}
