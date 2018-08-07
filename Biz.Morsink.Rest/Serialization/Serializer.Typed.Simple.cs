using Biz.Morsink.DataConvert;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    partial class Serializer<C>
    {
        partial class Typed<T>
        {
            public class Simple : Typed<T>
            {
                public Simple(Serializer<C> parent) : base(parent) {
                }
                public override SItem Serialize(C context, T item)
                    => new SValue(item);
                public override T Deserialize(C context, SItem item)
                    => item is SValue v && Parent.converter.Convert(v.Value).TryTo(out T t)
                        ? t
                        : throw new RestSerializationException($"Cannot deserialize the item into {typeof(T)}");
            }
        }
    }
}
