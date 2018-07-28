using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public partial class HalSerializer
    {
        private static string StripName(string name)
        {
            if (name.Contains('`'))
                return name.Substring(0, name.IndexOf('`'));
            else
                return name;
        }

        /// <summary>
        /// Abstract base class for serializers that handle a specific single type.
        /// </summary>
        /// <typeparam name="T">The type the serializer handles.</typeparam>
        public abstract partial class Typed<T> : IForType
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parent">A reference to the parent HalSerializer instance.</param>
            protected Typed(HalSerializer parent)
            {
                Parent = parent;
            }
            /// <summary>
            /// Should implement serialization for objects of type T.
            /// </summary>
            /// <param name="context">The applicable HalContext.</param>
            /// <param name="item">The object to serialize.</param>
            /// <returns>The serialization of the object as a JToken.</returns>
            public abstract JToken Serialize(HalContext context, T item);
            /// <summary>
            /// Should implement deserialization to objects of type T.
            /// </summary>
            /// <param name="context">The applicable HalContext.</param>
            /// <param name="token">The JToken to deserialize.</param>
            /// <returns>A deserialized object of type T.</returns>
            public abstract T Deserialize(HalContext context, JToken token);

            Type IForType.Type => typeof(T);
            JToken IForType.Serialize(HalContext context, object item) => Serialize(context, (T)item);
            object IForType.Deserialize(HalContext context, JToken token) => Deserialize(context, token);

            /// <summary>
            /// Contains a reference to the parent HalSerializer instance.
            /// </summary>
            protected HalSerializer Parent { get; }
        }
    }
}
