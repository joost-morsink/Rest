using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    partial class Serializer<C>
    {
        /// <summary>
        /// A serializer for the object type.
        /// Dynamically determines the serializer to use, based on the passed instance's type.
        /// This serializer is not able to deserialize.
        /// </summary>
        public class Object : Typed<object>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public Object(Serializer<C> parent) : base(parent)
            {
            }

            public override object Deserialize(C context, SItem item)
            {
                throw new NotSupportedException();
            }

            public override SItem Serialize(C context, object item)
            {
                if (item == null)
                    return SValue.Null;
                var ty = item.GetType();

                return ty == typeof(object) ? new SObject() : Parent.Serialize(context, ty, item);
            }
        }
    }
}
