using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    using static TypeDescriptorCreator;
    public class EnumDescriptorKind : IKind
    {
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

        
        public static bool IsOfKind(Type type)
            => type.IsEnum;
        bool IKind.IsOfKind(Type type)
            => IsOfKind(type);
        public class SerializerImpl<C, T> : Serializer<C>.Typed<T>
            where C : SerializationContext<C>
            where T : struct
        {
            public SerializerImpl(Serializer<C> parent) : base(parent)
            {
            }

            public override T Deserialize(C context, SItem item)
            {
                if (item is SValue val)
                {
                    return Enum.TryParse<T>(val.Value.ToString(), out var result) ? result : default;
                }
                else return default;
            }

            public override SItem Serialize(C context, T item)
            {
                return new SValue(item.ToString());
            }
        }
    }
}
