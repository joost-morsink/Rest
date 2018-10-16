using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    partial class Serializer<C>
    {
        /// <summary>
        /// An abstract base class for a specific serializer.
        /// </summary>
        /// <typeparam name="T">The type the serializer serializes.</typeparam>
        public abstract partial class Typed<T> : IForType
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parent">The parent serializer.</param>
            public Typed(Serializer<C> parent)
            {
                Parent = parent;
            }
            /// <summary>
            /// Gets the parent serializer.
            /// </summary>
            public Serializer<C> Parent { get; }

            /// <summary>
            /// The type this class serializes.
            /// </summary>
            public Type Type => typeof(T);

            /// <summary>
            /// Serializes an object to intermediate format.
            /// </summary>
            /// <param name="context">The serialization context.</param>
            /// <param name="item">The object to serialize.</param>
            /// <returns>An object in intermediate serialization format.</returns>
            public abstract SItem Serialize(C context, T item);
            /// <summary>
            /// Deserializes to an object from intermediate format.
            /// </summary>
            /// <param name="context">The serialization context.</param>
            /// <param name="item">An object in intermediate format.</param>
            /// <returns>A deserialized object.</returns>
            public abstract T Deserialize(C context, SItem item);

            SItem IForType.Serialize(C context, object item)
                => Serialize(context, (T)item);

            object IForType.Deserialize(C context, SItem item)
                => Deserialize(context, item);

            /// <summary>
            /// An abstract base class for serializers that implement serialization by constructing functions.
            /// </summary>
            public abstract class Func : Typed<T>
            {
                public const string SERIALIZE = "Serialize";
                public const string DESERIALIZE = "Deserialize";

                private readonly Func<C, T, SItem> serializer;
                private readonly Func<C, SItem, T> deserializer;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="parent">The parent serializer.</param>
                public Func(Serializer<C> parent) : base(parent)
                {
                    serializer = MakeSerializer();
                    deserializer = MakeDeserializer();
                }
                /// <summary>
                /// This method constructs the serializer function.
                /// </summary>
                /// <returns>THe serializer function.</returns>
                protected abstract Func<C, T, SItem> MakeSerializer();
                /// <summary>
                /// This method constructs the deserializer function.
                /// </summary>
                /// <returns>The deserializer function.</returns>
                protected abstract Func<C, SItem, T> MakeDeserializer();
                /// <summary>
                /// Serializes an object to intermediate format.
                /// </summary>
                /// <param name="context">The serialization context.</param>
                /// <param name="item">The object to serialize.</param>
                /// <returns>An object in intermediate serialization format.</returns>
                public override SItem Serialize(C context, T item)
                    => serializer(context, item);
                /// <summary>
                /// Deserializes to an object from intermediate format.
                /// </summary>
                /// <param name="context">The serialization context.</param>
                /// <param name="item">An object in intermediate format.</param>
                /// <returns>A deserialized object.</returns>
                public override T Deserialize(C context, SItem item)
                    => deserializer(context, item);
            }
        }

    }
}
