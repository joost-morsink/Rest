using Biz.Morsink.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class TaggedUnionRepresentation
    {
        public TaggedUnionRepresentation(string tag, object @object, TaggedUnionRepresentationType representationType)
        {
            Tag = tag;
            Object = @object;
            RepresentationType = representationType;
        }
        public string Tag { get; }
        public object Object { get; }
        public TaggedUnionRepresentationType RepresentationType { get; }
    }
    public class TaggedUnionRepresentationType
    {
        public TaggedUnionRepresentationType(IEnumerable<(string, Type)> tagMap)
        {
            Tags = tagMap.ToImmutableDictionary(t => t.Item2, t => t.Item1);
            Types = tagMap.ToImmutableDictionary(t => t.Item1, t => t.Item2);
        }
        public IReadOnlyDictionary<Type, string> Tags { get; }
        public IReadOnlyDictionary<string, Type> Types { get; }

        public TaggedUnionRepresentation Create(object o)
            => TryGetTag(o?.GetType(), out var tag)
                ? new TaggedUnionRepresentation(tag, o, this)
                : null;

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

    }
    public class TaggedUnionDescriptorKind : TypeDescriptorCreator.IKind
    {
        public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            throw new NotImplementedException();
        }

        public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
        {
            throw new NotImplementedException();
        }

        public bool IsOfKind(Type type)
        {
            throw new NotImplementedException();
        }
    }
    public abstract class TaggedUnionTypeRepresentation<T> : ITypeRepresentation
    {
        public TaggedUnionTypeRepresentation(TaggedUnionRepresentationType representationType)
        {
            RepresentationType = representationType;
        }

        public TaggedUnionRepresentationType RepresentationType { get; }

        public object GetRepresentable(object rep, Type specific)
        {
            throw new NotImplementedException();
        }

        public Type GetRepresentableType(Type type)
        {
            throw new NotImplementedException();
        }

        public object GetRepresentation(object obj)
        {
            throw new NotImplementedException();
        }

        public Type GetRepresentationType(Type type)
            => typeof(T).IsAssignableFrom(type) && RepresentationType.TryGetTag(type, out var tag)
                ? typeof(TaggedUnionRepresentation)
                : null;

        public bool IsRepresentable(Type type)
            => GetRepresentationType(type) != null;

        public bool IsRepresentation(Type type)
            => GetRepresentableType(type);

        protected abstract TaggedUnionRepresentationType GetRepresentationType();
    }
}
