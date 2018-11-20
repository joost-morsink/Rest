using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class EmbeddingRepresentation : SimpleTypeRepresentation<Embedding, object>
    {
        public override Embedding GetRepresentable(object representation)
            => null;

        public override object GetRepresentation(Embedding item)
            => item.Object;
    }
}
