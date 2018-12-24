using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    class TypeKeyedDictionaryKind : TypeDescriptorCreator.IKind
    {
        public static TypeKeyedDictionaryKind Instance { get; } = new TypeKeyedDictionaryKind();
        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            return TypeDescriptor.MakeDictionary("TypeKeyedDictionary", TypeDescriptor.MakeAny());
        }

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type)
            where C : SerializationContext<C>
        {
            if (type != typeof(TypeKeyedDictionary))
                return null;
            return new SerializerImpl<C>(serializer);
        }

        public bool IsOfKind(Type type)
            => type == typeof(TypeKeyedDictionary);

        private class SerializerImpl<C> : Serializer<C>.Typed<TypeKeyedDictionary>
            where C: SerializationContext<C>
        {
            public SerializerImpl(Serializer<C> parent) : base(parent)
            {

            }

            public override TypeKeyedDictionary Deserialize(C context, SItem item)
            {
                if (item is SObject obj)
                {
                    var dictionary = obj.Properties.Select(prop => new { Value = prop.Token, Type = Type.GetType(prop.Name) })
                        .Aggregate(TypeKeyedDictionary.Empty,
                        (dict, prop) => dict.SetUntyped(prop.Type, Parent.Deserialize(context, prop.Type, prop.Value)));
                    return dictionary;
                }
                else
                    return TypeKeyedDictionary.Empty;
            }

            public override SItem Serialize(C context, TypeKeyedDictionary item)
                => new SObject(item.AsEnumerable().Select(kvp =>
                    new SProperty(kvp.Key.AssemblyQualifiedName, Parent.Serialize(context, kvp.Key, kvp.Value))));
        }
    }
}
