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
    }
}
