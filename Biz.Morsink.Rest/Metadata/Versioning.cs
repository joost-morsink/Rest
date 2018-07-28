using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// Metadata class to carry version request and response metadata.
    /// </summary>
    public class Versioning
    {
        private readonly Lazy<VersionInRange> versions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="getter">A creator function to create the version information.</param>
        public Versioning(Func<VersionInRange> getter)
        {
            versions = new Lazy<VersionInRange>(getter);
        }

        /// <summary>
        /// Contains the current requested and/or responded version.
        /// </summary>
        public Version Current => versions.Value.Current;
        /// <summary>
        /// Contains a list of supported versions for some Rest path.
        /// </summary>
        public IReadOnlyList<Version> Supported => versions.Value.Supported;
        /// <summary>
        /// Returns a copy of this versioning instance, but without a current instance.
        /// </summary>
        /// <returns>A new versioning instance without a current version.</returns>
        public Versioning WithoutCurrent()
            => new Versioning(() => new VersionInRange(null, versions.Value.Supported));
    }
}
