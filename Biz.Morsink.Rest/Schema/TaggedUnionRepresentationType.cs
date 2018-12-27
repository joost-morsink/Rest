using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// An abstract base class for the metadata container for tagged union representations.
    /// </summary>
    public abstract class TaggedUnionRepresentationType
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseType">The base type for the tagged union.</param>
        /// <param name="tagMap">A collection of tag-type pairs.</param>
        protected TaggedUnionRepresentationType(Type baseType, params (string, Type)[] tagMap)
        {
            BaseType = baseType;
            Tags = tagMap.ToImmutableDictionary(t => t.Item2, t => t.Item1);
            Types = tagMap.ToImmutableDictionary(t => t.Item1, t => t.Item2);
        }
        /// <summary>
        /// Contains the tagged union's base type.
        /// </summary>
        public Type BaseType { get; }
        /// <summary>
        /// Contains a dictionary for looking up tags by type.
        /// </summary>
        public IReadOnlyDictionary<Type, string> Tags { get; }
        /// <summary>
        /// Contains a dictionary for looking up types by tag.
        /// </summary>
        public IReadOnlyDictionary<string, Type> Types { get; }
        /// <summary>
        /// Tries to get a tag for a type. Tries the entire inheritance chain.
        /// </summary>
        /// <param name="key">The type.</param>
        /// <param name="tag">The tag if found.</param>
        /// <returns>True if the tag was found, false otherwise.</returns>
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
        /// <summary>
        /// Tries to get the type for a tag. 
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="type">The type if found.</param>
        /// <returns>True if the tag was found, false otherwise.</returns>
        public bool TryGetType(string tag, out Type type)
            => Types.TryGetValue(tag, out type);
    }
}
