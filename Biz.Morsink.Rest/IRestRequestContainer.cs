using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Interface for containers of a Rest request.
    /// Primarily used for having access to the raw request from a repository context.
    /// </summary>
    public interface IRestRequestContainer
    {
        /// <summary>
        /// Gets the raw Rest request this repository instance was constructed for.
        /// </summary>
        RestRequest Request { get; set; }
    }
}
