using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Enum specifying a kind of entity.
    /// It is used in conjunction with RestFailures to indicate what kind of entity was responsible for the failure returned.
    /// </summary>
    public enum RestEntityKind
    {
        /// <summary>
        /// General. Unspecified entity kind.
        /// </summary>
        General,
        /// <summary>
        /// Repository entity kind.
        /// </summary>
        Repository,
        /// <summary>
        /// Capability entity kind.
        /// </summary>
        Capability,
        /// <summary>
        /// Resource entity kind.
        /// </summary>
        Resource
    }
}
