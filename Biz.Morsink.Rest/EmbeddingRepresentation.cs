using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A type representation for Embedding using the embedded object as the representation.
    /// This type representation is not invertible.
    /// </summary>
    public class EmbeddingRepresentation : SimpleTypeRepresentation<Embedding, object>
    {
        public override Embedding GetRepresentable(object representation)
            => null;

        public override object GetRepresentation(Embedding item)
            => item.Object;
    }
}
