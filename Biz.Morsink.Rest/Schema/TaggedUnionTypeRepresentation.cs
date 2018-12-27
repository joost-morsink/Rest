using System;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// An ITypeRepresentation for tagged union representations.
    /// </summary>
    /// <typeparam name="T">The base type of the representable types.</typeparam>
    /// <typeparam name="TRepType">The representation's metadata type.</typeparam>
    public class TaggedUnionTypeRepresentation<T, TRepType> : ITypeRepresentation
        where TRepType : TaggedUnionRepresentationType, new()
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TaggedUnionTypeRepresentation()
        {
            RepresentationType = new TRepType();
        }
        /// <summary>
        /// Contains the metadata instance.
        /// </summary>
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
            => typeof(T) == type
                ? typeof(TaggedUnionRepresentation<TRepType>)
                : null;

        public bool IsRepresentable(Type type)
            => GetRepresentationType(type) != null;

        public bool IsRepresentation(Type type)
            => GetRepresentableType(type) != null;

    }
}
