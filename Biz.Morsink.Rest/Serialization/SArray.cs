using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    public class SArray : SItem
    {
        public SArray(IEnumerable<SItem> content)
        {
            Content = content as SItem[] ?? content.ToArray();
        }
        public SArray(params SItem[] content) : this((IEnumerable<SItem>)content)
        { }

        public IReadOnlyList<SItem> Content { get; }

        public override int GetHashCode()
            => Content.Aggregate(0, (acc, item) => acc ^ item.GetHashCode());
        public override bool Equals(SItem other)
            => other is SArray arr && Equals(arr);
        public bool Equals(SArray other)
            => Content.Count == other.Content.Count && Content.SequenceEqual(other.Content);
    }
}
