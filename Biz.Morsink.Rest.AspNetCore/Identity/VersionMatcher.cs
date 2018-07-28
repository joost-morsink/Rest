using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    /// <summary>
    /// This struct can match versions of a Rest repository.
    /// </summary>
    public struct VersionMatcher
    {
        /// <summary>
        /// Creates a new VersionMatcher that matches on a specific major version.
        /// </summary>
        /// <param name="major">The major version component.</param>
        /// <returns>A new VersionMatcher that matches the specified version.</returns>
        public static VersionMatcher OnMajor(int major)
            => new VersionMatcher(major, false, false);
        /// <summary>
        /// Contains a VersionMatcher that always matches the oldest version.
        /// </summary>
        public static VersionMatcher Oldest => new VersionMatcher(null, true, false);
        /// <summary>
        /// Contains a VersionMatcher that always matches the newest version.
        /// </summary>
        public static VersionMatcher Newest => new VersionMatcher(null, false, true);

        private VersionMatcher(int? major, bool oldest, bool latest)
        {
            this.major = major;
            this.oldest = oldest;
            this.latest = latest;
        }

        private readonly int? major;
        private readonly bool oldest;
        private readonly bool latest;

        /// <summary>
        /// Tries to match a version from a collection of versions.
        /// </summary>
        /// <param name="versions">A collection of versions.</param>
        /// <returns>A matching version, null if none match.</returns>
        public Version Match(IEnumerable<Version> versions)
        {
            var major = this.major;
            if (major.HasValue)
                return versions.Where(v => v.Major == major).FirstOrDefault();
            else if (latest)
                return versions.OrderByDescending(v => v.Major).FirstOrDefault();
            else  // oldest is true  (or false for default VersionMatchers)
                return versions.OrderBy(v => v.Major).FirstOrDefault();
        }
        /// <summary>
        /// Tries to match a version from a collection of versions.
        /// </summary>
        /// <param name="versions">A collection of versions, paired with some data element.</param>
        /// <returns>A matching version along with the corresponding data element, null if none match.</returns>
        public (Version, T) Match<T>(IEnumerable<(Version, T)> versions)
        {
            var major = this.major;
            if (major.HasValue)
                return versions.Where(v => v.Item1.Major == major).FirstOrDefault();
            else if (latest)
                return versions.OrderByDescending(v => v.Item1.Major).FirstOrDefault();
            else  // oldest is true  (or false for default VersionMatchers)
                return versions.OrderBy(v => v.Item1.Major).FirstOrDefault();

        }
    }

}
