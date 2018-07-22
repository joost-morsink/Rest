using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    public struct VersionMatcher
    {
        public static VersionMatcher OnMajor(int major)
            => new VersionMatcher(major, false, false);
        public static VersionMatcher Oldest => new VersionMatcher(null, true, false);
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
