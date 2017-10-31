using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Enumeration for types of Rest redirection.
    /// </summary>
    public enum RestRedirectType
    {
        /// <summary>
        /// A permanent redirect type.
        /// </summary>
        Permanent,
        /// <summary>
        /// A temporary redirect type.
        /// </summary>
        Temporary,
        /// <summary>
        /// Response is not necessary, because data is already available client-side.
        /// Mathematically, this is a redirect to 'void'.
        /// </summary>
        NotNecessary
    }
}
