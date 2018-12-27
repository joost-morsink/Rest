using Biz.Morsink.Rest.Serialization;
using Biz.Morsink.Rest.Utils;
using System;
using System.Linq;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A TypeDescriptorCreator.IKind implementation for tagged union representations.
    /// </summary>
    public class TaggedUnionDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// A singleton instance fot the kind.
        /// </summary>
        public static TaggedUnionDescriptorKind Instance { get; } = new TaggedUnionDescriptorKind();
        /// <summary>
        /// Constructor.
        /// </summary>
        private TaggedUnionDescriptorKind() { }

        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {

            var reprType = GetRepresentationType(context.Type);
            if (reprType == null)
                return null;
            var repr = (TaggedUnionRepresentationType)Activator.CreateInstance(reprType);
            return TypeDescriptor.MakeUnion(creator.GetTypeName(repr.BaseType),
                repr.Types.Select(kvp => new TypeDescriptor.Record(kvp.Key, new[] {
                    new PropertyDescriptor<TypeDescriptor>(kvp.Key, RecordDescriptorKind.Instance.GetDescriptor(creator, new TypeDescriptorCreator.Context(repr.Types[kvp.Key], repr.BaseType)), true)
                }, null)), repr.BaseType);
        }

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
        {
            var reprType = GetRepresentationType(type);
            if (reprType == null)
                return null;
            else
                return (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), reprType), serializer);
        }
        private class SerializerImpl<C, TRepType> : Serializer<C>.Typed<TaggedUnionRepresentation<TRepType>>.Simple
            where C : SerializationContext<C>
            where TRepType : TaggedUnionRepresentationType, new()
        {
            private static readonly TRepType repType = new TRepType();
            public SerializerImpl(Serializer<C> parent) : base(parent)
            {
            }
            public override SItem Serialize(C context, TaggedUnionRepresentation<TRepType> item)
            {
                return item.RepresentationType.TryGetType(item.Tag, out var type)
                      ? new SObject(new SProperty(item.Tag, Parent.Serialize(context, type, item.Object)))
                      : throw new RestSerializationException("Cannot serialize tagged union.");
            }
            public override TaggedUnionRepresentation<TRepType> Deserialize(C context, SItem item)
            {
                if (item is SObject sobj)
                {
                    var prop = sobj.Properties.First();
                    var tag = prop.Name;
                    if (repType.TryGetType(tag, out var type))
                    {
                        var obj = Parent.Deserialize(context, type, prop.Token);
                        return new TaggedUnionRepresentation<TRepType>(tag, obj, repType);
                    }
                }
                throw new RestSerializationException("Cannot deserialize tagged union.");
            }
        }

        private Type GetRepresentationType(Type type)
            => type.GetGeneric(typeof(TaggedUnionRepresentation<>));
        /// <summary>
        /// Checks if a type is of the kind tagged union.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True is the type is a tagged union.</returns>
        public bool IsOfKind(Type type)
            => typeof(TaggedUnionRepresentation).IsAssignableFrom(type) && GetRepresentationType(type) != null;
 
    }
}
