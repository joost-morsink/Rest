using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// A struct that contains the actual version metadata.
    /// </summary>
    public struct VersionInRange
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="current">The current version.</param>
        /// <param name="supported">A list of all the supported versions.</param>
        public VersionInRange(Version current, IEnumerable<Version> supported)
        {
            Current = current;
            Supported = supported.ToArray();
        }
        /// <summary>
        /// The current version.
        /// </summary>
        public Version Current { get; }
        /// <summary>
        /// A list of all the supported versions by some Rest path.
        /// </summary>
        public IReadOnlyList<Version> Supported { get; }
    }
}
