using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    /// <summary>
    /// Service interface for accessing a scoped instance of a RestPrefixContainer.
    /// </summary>
    public interface IRestPrefixContainerAccessor
    {
        /// <summary>
        /// Gets the currently in scope RestPrefixContainer.
        /// </summary>
        RestPrefixContainer RestPrefixContainer { get; }
    }
}
