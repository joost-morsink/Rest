using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    using static TypeDescriptorCreator;
    /// <summary>
    /// This class represents all enum types.
    /// </summary>
    public class EnumDescriptorKind : IKind
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static EnumDescriptorKind Instance { get; } = new EnumDescriptorKind();
        
        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (!context.Type.IsEnum)
                return null;
            return TypeDescriptor.MakeUnion(GetTypeName(context.Type),
                Enum.GetNames(context.Type).Select(name => TypeDescriptor.MakeValue(TypeDescriptor.MakeString(), name)),
                context.Type); ;
        }

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
            => IsOfKind(type)
                ? (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), type), serializer)
                : null;

        /// <summary>
        /// Checks if the specified type is an enum type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the specified type is an enum type, false otherwise.</returns>
        public static bool IsOfKind(Type type)
            => type.IsEnum;
        bool IKind.IsOfKind(Type type)
            => IsOfKind(type);
        /// <summary>
        /// Serializer implementation.
        /// </summary>
        /// <typeparam name="C">The serialization context type.</typeparam>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public class SerializerImpl<C, T> : Serializer<C>.Typed<T>
            where C : SerializationContext<C>
            where T : struct
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parent">The parent serializer.</param>
            public SerializerImpl(Serializer<C> parent) : base(parent)
            {
            }

            /// <summary>
            /// Deserializes an SItem to an object of type T.
            /// </summary>
            /// <param name="context">The serialization context.</param>
            /// <param name="item">The item to deserialize.</param>
            /// <returns>A deserialized object.</returns>
            public override T Deserialize(C context, SItem item)
            {
                if (item is SValue val)
                {
                    return Enum.TryParse<T>(val.Value.ToString(), out var result) ? result : default;
                }
                else return default;
            }
            /// <summary>
            /// Serializes an object of type T to an SItem.
            /// </summary>
            /// <param name="context">The serialization context.</param>
            /// <param name="item">The item to serialize.</param>
            /// <returns></returns>
            public override SItem Serialize(C context, T item)
            {
                return new SValue(item.ToString());
            }
        }
    }
}
