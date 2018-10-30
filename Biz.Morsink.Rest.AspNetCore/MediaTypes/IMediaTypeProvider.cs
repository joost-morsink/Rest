using System;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// A provider interface for media types.
    /// </summary>
    public interface IMediaTypeProvider
    {
        /// <summary>
        /// Check for an available media type.
        /// </summary>
        /// <param name="original">THe original type of object in the result.</param>
        /// <param name="representation">The representation type of the original. 
        /// If there is no representation, this value should equal the 'original' parameter.</param>
        /// <returns>A media type if one could be found, null otherwise.</returns>
        string GetMediaType(Type original, Type representation);
    }
}
