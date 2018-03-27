using Biz.Morsink.Rest.AspNetCore.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    /// <summary>
    /// A container for Rest prefix mappings.
    /// </summary>
    public class RestPrefixContainer : IEnumerable<RestPrefix>
    {
        private ImmutableDictionary<string, RestPrefix> byPrefix;
        private ImmutableDictionary<string, RestPrefix> byAbbrev;
        private PrefixMatcher<RestPrefix> prefixMatcher;
        private int idCounter;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RestPrefixContainer() {
            Clear();
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prefixes">A collection of prefixes to register in the container.</param>
        public RestPrefixContainer(IEnumerable<RestPrefix> prefixes)
        {
            byPrefix = prefixes.ToImmutableDictionary(p => p.Prefix);
            byAbbrev = prefixes.ToImmutableDictionary(p => p.Abbreviation);
            prefixMatcher = prefixes.Aggregate(PrefixMatcher<RestPrefix>.Empty, (m, p) => m.Add(p.Prefix, p));
            idCounter = 0;
        }
        private RestPrefixContainer(RestPrefixContainer container)
        {
            byPrefix = container.byPrefix;
            byAbbrev = container.byAbbrev;
            prefixMatcher = container.prefixMatcher;
            idCounter = container.idCounter;
        }
        /// <summary>
        /// Makes a copy of this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public RestPrefixContainer Copy()
            => new RestPrefixContainer(this);
        /// <summary>
        /// Registers a RestPrefix in the container.
        /// </summary>
        /// <param name="prefix"></param>
        public void Register(RestPrefix prefix)
        {
            if (byAbbrev.TryGetValue(prefix.Abbreviation,out var existing))
            {
                byAbbrev = byAbbrev.Remove(existing.Abbreviation);
                byPrefix = byPrefix.Remove(existing.Prefix);
            }
            byPrefix = byPrefix.SetItem(prefix.Prefix, prefix);
            byAbbrev = byAbbrev.SetItem(prefix.Abbreviation, prefix);
            prefixMatcher = prefixMatcher.Add(prefix.Prefix, prefix);
        }
        /// <summary>
        /// Gets a prefix entry for the given prefix.
        /// Creates a new prefix if the prefix is not found.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>A RestPrefix entry from the current container.</returns>
        public RestPrefix GetPrefix(string prefix)
        {
            if (!byPrefix.ContainsKey(prefix))
                Register(new RestPrefix(prefix, NextId()));
            return byPrefix[prefix];
        }
        /// <summary>
        /// Tries to find a prefix belonging to a certain abbreviation.
        /// </summary>
        /// <param name="abbreviation">The abbreviation to search for.</param>
        /// <param name="result">If the abbreviation was found this parameter is set to the RestPrefix, otherwise null.</param>
        /// <returns>A boolean indicating whether the abbreviation was found.</returns>
        public bool TryGetByAbbreviation(string abbreviation, out RestPrefix result)
            => byAbbrev.TryGetValue(abbreviation, out result);
        /// <summary>
        /// Tries to find a matching prefix for a certain address.
        /// </summary>
        /// <param name="address">The address to match.</param>
        /// <param name="result">If the address could be matched to a prefix, this parameter is set to the RestPrefix, otherwise null.</param>
        /// <returns>A boolean indicating whether the address could be matched against a prefix.</returns>
        public bool TryMatch(string address, out RestPrefix result)
            => prefixMatcher.TryMatch(address, out result);

        /// <summary>
        /// Clears all registrations for this RestPrefixContainer.
        /// </summary>
        public void Clear()
        {
            byPrefix = ImmutableDictionary<string, RestPrefix>.Empty;
            byAbbrev = ImmutableDictionary<string, RestPrefix>.Empty;
            prefixMatcher = PrefixMatcher<RestPrefix>.Empty;
            idCounter = 0;
        }
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

            
        public IEnumerator<RestPrefix> GetEnumerator()
            => byAbbrev.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
