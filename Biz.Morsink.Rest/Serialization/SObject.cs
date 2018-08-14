using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    public class SObject : SItem
    {
        public SObject(params SProperty[] properties) : this((IEnumerable<SProperty>)properties)
        { }

        public SObject(IEnumerable<SProperty> properties)
        {
            Properties = properties.ToArray();
        }

        public IReadOnlyList<SProperty> Properties { get; }

        public Dictionary<string, SItem> ToDictionary(IEqualityComparer<string> equalityComparer = null)
            => Properties.ToDictionary(p => p.Name, p => p.Token, equalityComparer ?? EqualityComparer<string>.Default);
    }
}
