using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    partial class Serializer<C>
    {
        public abstract partial class Typed<T> : IForType
        {
            public Typed(Serializer<C> parent)
            {
                Parent = parent;
            }

            public Serializer<C> Parent { get; }

            public Type Type => throw new NotImplementedException();

            public abstract SItem Serialize(C context, T item);
            public abstract T Deserialize(C context, SItem item);

            SItem IForType.Serialize(C context, object item)
                => Serialize(context, (T)item);

            object IForType.Deserialize(C context, SItem item)
                => Deserialize(context, item);

            public abstract class Func : Typed<T>
            {
                public const string SERIALIZE = "Serialize";
                public const string DESERIALIZE = "Deserialize";

                private readonly Func<C, T, SItem> serializer;
                private readonly Func<C, SItem, T> deserializer;

                public Func(Serializer<C> parent) : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }
                protected abstract Func<C, T, SItem> MakeSerializer();
                protected abstract Func<C, SItem, T> MakeDeserializer();
                public override SItem Serialize(C context, T item)
                    => serializer(context, item);
                public override T Deserialize(C context, SItem item)
                    => deserializer(context, item);
            }
        }

    }
}
