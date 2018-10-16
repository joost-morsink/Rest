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
            /// <summary>
            /// This serializer simply wraps the value in an SValue.
            /// </summary>
            public class Simple : Typed<T>
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">The parent serializer.</param>
                public Simple(Serializer<C> parent) : base(parent) {
                }
                public override SItem Serialize(C context, T item)
                    => new SValue(item);
                public override T Deserialize(C context, SItem item)
                    => item is SValue v && Parent.Converter.Convert(v.Value).TryTo(out T t)
                        ? t
                        : throw new RestSerializationException($"Cannot deserialize the item into {typeof(T)}");
            }
        }
    }
}
