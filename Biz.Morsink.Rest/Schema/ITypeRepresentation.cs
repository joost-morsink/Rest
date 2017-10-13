using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This interface allows for complex types to have a simple representation type.
    /// </summary>
    public interface ITypeRepresentation
    {
        /// <summary>
        /// Determines if some type is representable using this ITypeRepresentation.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is representable using this ITypeRepresentation.</returns>
        bool IsRepresentable(Type type);
        /// <summary>
        /// Determines if some type is the representation of a representable type for this ITypeRepresentation.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a representation fo a representable type using this ITypeRepresentation.</returns>
        bool IsRepresentation(Type type);
        /// <summary>
        /// Gets the representation type for a representable type.
        /// </summary>
        /// <param name="type">The type to get a representation type for.</param>
        /// <returns>A representation type for the representable type. Null if the type is not representable.</returns>
        Type GetRepresentationType(Type type);
        /// <summary>
        /// Gets the representable type for a representation type.
        /// </summary>
        /// <param name="type">The type to get a representable type for.</param>
        /// <returns>A representable type for the representation type. Null if the type is not a representation.</returns>
        Type GetRepresentableType(Type type);
        /// <summary>
        /// Gets the representation of some representable instance.
        /// </summary>
        /// <param name="obj">The object to represent.</param>
        /// <returns>A representation for the representable instance.</returns>
        object GetRepresentation(object obj);
        /// <summary>
        /// Gets the representable object of some representation instance.
        /// </summary>
        /// <param name="rep">The representation.</param>
        /// <returns>A representable object for the representation instance.</returns>
        object GetRepresentable(object rep);
    }
}
