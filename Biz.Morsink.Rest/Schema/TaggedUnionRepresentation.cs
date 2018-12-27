using Biz.Morsink.Rest.Serialization;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// Base representation class for tagged unions.
    /// </summary>
    public abstract class TaggedUnionRepresentation
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="object">An object of the type corresponding tot the tag.</param>
        /// <param name="representationType">The metadata instance for the representation type.</param>
        public TaggedUnionRepresentation(string tag, object @object, TaggedUnionRepresentationType representationType)
        {
            Tag = tag;
            Object = @object;
            RepresentationType = representationType;
        }
        /// <summary>
        /// Contains the Tag for the value.
        /// </summary>
        public string Tag { get; }
        /// <summary>
        /// Contains the represented object.
        /// </summary>
        public object Object { get; }
        /// <summary>
        /// Contains the metadata instaance fot the representation type.
        /// </summary>
        public TaggedUnionRepresentationType RepresentationType { get; }
    }
    /// <summary>
    /// A representation class for a tagged union.
    /// </summary>
    /// <typeparam name="TRepType">A TaggedUnionRepresentationType containing all the metadata for the tagged union.</typeparam>
    public sealed class TaggedUnionRepresentation<TRepType> : TaggedUnionRepresentation
        where TRepType : TaggedUnionRepresentationType, new()
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="object">An object of the type corresponding tot the tag.</param>
        /// <param name="type">The metadata instance for the representation type.</param>
        public TaggedUnionRepresentation(string tag, object @object, TRepType type = null)
            : base(tag, @object, type ?? new TRepType())
        { }
    }
}
