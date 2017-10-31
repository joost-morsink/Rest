using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface for Rest results that represent redirects.
    /// </summary>
    public interface IRestRedirect : IRestResult
    {
        /// <summary>
        /// Gets the type of redirect.
        /// </summary>
        RestRedirectType Type { get; }
        /// <summary>
        /// Gets the target of the redirect.
        /// </summary>
        IIdentity Target { get; }
    }
}
