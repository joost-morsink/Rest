using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    /// <summary>
    /// A Type representation implementation for the Version class.
    /// Versions are represented by their version string. 
    /// (Instead of all their component properties.)
    /// </summary>
    public class VersionRepresentation : SimpleTypeRepresentation<Version, string>
    {
        public override Version GetRepresentable(string representation)
            => representation.Contains(".")
                ? Version.TryParse(representation, out var ver) ? ver : null
                : int.TryParse(representation, out var maj) ? new Version(maj, 0) : null;

        public override string GetRepresentation(Version item)
            => item.ToString();
    }
}
