using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    public class Versioning
    {
        private readonly Lazy<VersionInRange> versions;

        public Versioning(Func<VersionInRange> getter)
        {
            versions = new Lazy<VersionInRange>(getter);
        }

        public Version Current => versions.Value.Current;
        public IReadOnlyList<Version> Supported => versions.Value.Supported;
    }
}
