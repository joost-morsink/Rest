using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.MediaTypes
{
    /// <summary>
    /// Apply this attribute to a class if a media type should be set for this type of class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MediaTypeAttribute : Attribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mediaType">The media type.</param>
        public MediaTypeAttribute(string mediaType)
        {
            MediaType = mediaType;
        }
        /// <summary>
        /// Contains the media type that is applicable to the attributed class.
        /// </summary>
        public string MediaType { get; }
    }
}
