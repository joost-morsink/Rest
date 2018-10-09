using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// A kind for Rest serialization objects. 
    /// Applies to SItem, SObject, SArray and SValue.
    /// </summary>
    public class SerializationKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// A singleton instance of the SerializationKind class.
        /// </summary>
        public static SerializationKind Instance { get; } = new SerializationKind();
        private SerializationKind() { }

        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (!IsOfKind(context.Type))
                return null;
            if (context.Type == typeof(SObject))
                return TypeDescriptor.MakeRecord("SObject", Enumerable.Empty<PropertyDescriptor<TypeDescriptor>>(), typeof(SObject));
            else if (context.Type == typeof(SArray))
                return TypeDescriptor.MakeArray(TypeDescriptor.MakeAny());
            else if (context.Type == typeof(SValue))
                return TypeDescriptor.MakeAny();
            else
                throw new ArgumentException("SItem derived type unknown.");
        }

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
        {
            if (!IsOfKind(type))
                return null;
            return (Serializer<C>.IForType)Activator.CreateInstance(typeof(SItemSerializer<,>).MakeGenericType(typeof(C), type), serializer);
        }
        bool TypeDescriptorCreator.IKind.IsOfKind(Type type)
            => IsOfKind(type);

        /// <summary>
        /// Determines whether the type is of the serialization kind.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is of the serialization kind.</returns>
        public static bool IsOfKind(Type type)
            => typeof(SItem).IsAssignableFrom(type);

        
        private class SItemSerializer<C, S> : Serializer<C>.Typed<S>
            where C : SerializationContext<C>
            where S : SItem
        {
            public SItemSerializer(Serializer<C> parent) : base(parent)
            {
            }

            public override S Deserialize(C context, SItem item)
                => item as S;

            public override SItem Serialize(C context, S item)
                => item;
        }

    }
}
