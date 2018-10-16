using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// An abstract base class for type representations that need direction when converted back to a representable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    public abstract class TypeDirectedTypeRepresentation<T, R> : ITypeRepresentation
    {
        /// <summary>
        /// Gets a representation for the specified object.
        /// </summary>
        /// <param name="item">The object to represent.</param>
        /// <returns>A representation for the specified object.</returns>
        public abstract R GetRepresentation(T item);
        /// <summary>
        /// Gets a representable that is represented by the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <param name="specific">The type direction for getting a representable.</param>
        /// <returns>An object that can be represented by the representation, of the specified type.</returns>
        public abstract T GetRepresentable(R representation, Type specific);

        public virtual Type GetRepresentableType(Type type)
            => type == typeof(R) ? typeof(T) : null;
        public virtual Type GetRepresentationType(Type type)
            => typeof(T).IsAssignableFrom(type) ? typeof(R) : null;

        object ITypeRepresentation.GetRepresentable(object rep, Type specific)
            => rep is R r ? GetRepresentable(r, specific) : default;

        object ITypeRepresentation.GetRepresentation(object obj)
            => obj is T t ? GetRepresentation(t) : default;

        bool ITypeRepresentation.IsRepresentable(Type type)
            => GetRepresentationType(type) != null;

        bool ITypeRepresentation.IsRepresentation(Type type)
            => GetRepresentableType(type) != null;
    }
}
