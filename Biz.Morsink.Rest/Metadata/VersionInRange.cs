using System;
using System.Collections.Generic;
using System.Linq;

namespace Biz.Morsink.Rest.Metadata
{
    public struct VersionInRange
    {
        public VersionInRange(Version current, IEnumerable<Version> supported)
        {
            Current = current;
            Supported = supported.ToArray();
        }
        public Version Current { get; }
        public IReadOnlyList<Version> Supported { get; }
    }
}
