using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// An array of itens.
    /// </summary>
    public class SArray : SItem
    {
        public static SArray Empty { get; } = new SArray();
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="content">Items to be contained in the array.</param>
        public SArray(IEnumerable<SItem> content)
        {
            Content = content as SItem[] ?? content.ToArray();
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="content">Items to be contained in the array.</param>
        public SArray(params SItem[] content) : this((IEnumerable<SItem>)content)
        { }

        /// <summary>
        /// A list of items contained in this array.
        /// </summary>
        public IReadOnlyList<SItem> Content { get; }

        public override int GetHashCode()
            => Content.Aggregate(0, (acc, item) => acc ^ item.GetHashCode());
        public override bool Equals(SItem other)
            => other is SArray arr && Equals(arr);
        public bool Equals(SArray other)
            => Content.Count == other.Content.Count && Content.SequenceEqual(other.Content);
        protected internal override string ToString(int indent)
            => $"{NewLine(indent)}[{string.Join(NewLine(indent + 2), Content.Select(c => c.ToString(indent+2)))}{NewLine(indent)}]";

    }
}
