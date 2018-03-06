using Biz.Morsink.Rest.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    public class RestPrefixContainer
    {
        private ImmutableDictionary<string, RestPrefix> byPrefix;
        private ImmutableDictionary<string, RestPrefix> byAbbrev;
        private int idCounter;

        public RestPrefixContainer() {
            byPrefix = ImmutableDictionary<string, RestPrefix>.Empty;
            byAbbrev = ImmutableDictionary<string, RestPrefix>.Empty;
            idCounter = 0;
        }
        public RestPrefixContainer(IEnumerable<RestPrefix> prefixes)
        {
            byPrefix = prefixes.ToImmutableDictionary(p => p.Prefix);
            byAbbrev = prefixes.ToImmutableDictionary(p => p.Abbreviation);
            idCounter = 0;
        }
        private RestPrefixContainer(RestPrefixContainer container)
        {
            byPrefix = container.byPrefix;
            byAbbrev = container.byAbbrev;
            idCounter = container.idCounter;
        }
        public RestPrefixContainer Copy()
            => new RestPrefixContainer(this);

        public void Register(RestPrefix prefix)
        {
            if (byAbbrev.TryGetValue(prefix.Abbreviation,out var existing))
            {
                byAbbrev = byAbbrev.Remove(existing.Abbreviation);
                byPrefix = byPrefix.Remove(existing.Prefix);
            }
            byPrefix = byPrefix.SetItem(prefix.Prefix, prefix);
            byAbbrev = byAbbrev.SetItem(prefix.Abbreviation, prefix);
        }
        public RestPrefix GetPrefix(string prefix)
        {
            if (!byPrefix.ContainsKey(prefix))
                Register(new RestPrefix(prefix, NextId()));
            return byPrefix[prefix];
        }
        public bool TryGetByAbbreviation(string abbreviation, out RestPrefix result)
            => byAbbrev.TryGetValue(abbreviation, out result);

        private string NextId()
        {
            var n = idCounter++;
            string str = null;
            while (n > 0)
            {
                str += 'a' + n % 26;
                n /= 26;
            }
            var res = str ?? "a";
            return byAbbrev.ContainsKey(res) ? NextId() : res;
        }

    }
}
