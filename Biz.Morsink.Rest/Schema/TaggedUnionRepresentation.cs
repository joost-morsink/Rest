using Biz.Morsink.Rest.Serialization;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public abstract class TaggedUnionRepresentation
    {
        public TaggedUnionRepresentation(string tag, object @object, TaggedUnionRepresentationType representationType)
        {
            Tag = tag;
            Object = @object;
            RepresentationType = representationType;
        }
        public string Tag { get; }
        public object Object { get; }
        public SItem Item { get; }
        public TaggedUnionRepresentationType RepresentationType { get; }
    }
    public sealed class TaggedUnionRepresentation<TRepType> : TaggedUnionRepresentation
        where TRepType : TaggedUnionRepresentationType, new()
    {
        public TaggedUnionRepresentation(string tag, object @object, TRepType type = null)
            : base(tag, @object, type ?? new TRepType())
        { }
    }
    public abstract class TaggedUnionRepresentationType
    {
        protected TaggedUnionRepresentationType(Type baseType, params (string, Type)[] tagMap)
        {
            BaseType = baseType;
            Tags = tagMap.ToImmutableDictionary(t => t.Item2, t => t.Item1);
            Types = tagMap.ToImmutableDictionary(t => t.Item1, t => t.Item2);
        }
        public Type BaseType { get; }
        public IReadOnlyDictionary<Type, string> Tags { get; }
        public IReadOnlyDictionary<string, Type> Types { get; }

        public bool TryGetTag(Type key, out string tag)
        {
            tag = default;
            while (key != null)
            {
                if (Tags.TryGetValue(key, out tag))
                    return true;
                key = key.BaseType;
            }
            return false;
        }
        public bool TryGetType(string tag, out Type type)
            => Types.TryGetValue(tag, out type);
    }
    public class TaggedUnionDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static TaggedUnionDescriptorKind Instance { get; } = new TaggedUnionDescriptorKind();
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
                return (Serializer<C>.IForType)Activator.CreateInstance(typeof(SerializerImpl<,>).MakeGenericType(typeof(C), reprType));
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
        public bool IsOfKind(Type type)
            => typeof(TaggedUnionRepresentation).IsAssignableFrom(type) && GetRepresentationType(type) != null;
    }
    public class TaggedUnionTypeRepresentation<T, TRepType> : ITypeRepresentation
        where TRepType : TaggedUnionRepresentationType, new()
    {
        public TaggedUnionTypeRepresentation()
        {
            RepresentationType = new TRepType();
        }

        public TRepType RepresentationType { get; }

        public object GetRepresentable(object rep, Type specific)
            => (rep as TaggedUnionRepresentation)?.Object;

        public Type GetRepresentableType(Type type)
            => type == typeof(TaggedUnionRepresentation) ? typeof(T) : null;

        public object GetRepresentation(object obj)
            => RepresentationType.TryGetTag(obj.GetType(), out var tag)
                ? new TaggedUnionRepresentation<TRepType>(tag, obj, RepresentationType)
                : null;

        public Type GetRepresentationType(Type type)
            => typeof(T).IsAssignableFrom(type)
                ? typeof(TaggedUnionRepresentation<TRepType>)
                : null;

        public bool IsRepresentable(Type type)
            => GetRepresentationType(type) != null;

        public bool IsRepresentation(Type type)
            => GetRepresentableType(type) != null;

    }
}
