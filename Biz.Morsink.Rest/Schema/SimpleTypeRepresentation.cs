using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// An abstract base class for type representations with a single type.
    /// </summary>
    /// <typeparam name="T">The representable type.</typeparam>
    /// <typeparam name="R">The representation type.</typeparam>
    public abstract class SimpleTypeRepresentation<T, R> : ITypeRepresentation
    {
        /// <summary>
        /// Gets a representation for an object.
        /// </summary>
        /// <param name="item">The object to get a representation for.</param>
        /// <returns>A representation of the specified object.</returns>
        public abstract R GetRepresentation(T item);
        /// <summary>
        /// Gets the representable object back from a representation object.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>An object that was represented by the specified representation.</returns>
        public abstract T GetRepresentable(R representation);

        public virtual Type GetRepresentableType(Type type)
            => type == typeof(R) ? typeof(T) : null;
        public virtual Type GetRepresentationType(Type type)
            => typeof(T).IsAssignableFrom(type) ? typeof(R) : null;

        object ITypeRepresentation.GetRepresentable(object rep, Type specific)
            => rep is R r ? GetRepresentable(r) : default;

        object ITypeRepresentation.GetRepresentation(object obj)
            => obj is T t ? GetRepresentation(t) : default;

        bool ITypeRepresentation.IsRepresentable(Type type)
            => GetRepresentationType(type) != null;

        bool ITypeRepresentation.IsRepresentation(Type type)
            => GetRepresentableType(type) != null;
    }
}
