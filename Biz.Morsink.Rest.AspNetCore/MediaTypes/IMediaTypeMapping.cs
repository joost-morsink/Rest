using System;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// Interface for a media type mapping.
    /// </summary>
    public interface IMediaTypeMapping
    {
        /// <summary>
        /// Returns true if this mapping applies to the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the mapping applies, false otherwise.</returns>
        bool Applies(Type type);
        /// <summary>
        /// Gets the media type for the specified type.
        /// </summary>
        /// <param name="type">The type to get the media type for.</param>
        /// <returns>If Applies returns true for the given type this method returns the correct media type for the specified type.
        /// If Applies returns false for the given type, the output for this function is unspecified.</returns>
        string GetMediaType(Type type);
    }
}
