using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Delegate type for a RestRequest handler.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>A possibly asynchronous RestResponse.</returns>
    public delegate ValueTask<RestResponse> RestRequestHandlerDelegate(RestRequest request);
    /// <summary>
    /// Interface (for dependency injection purposes) for the complete Rest Request handler pipeline.
    /// </summary>
    public interface IRestRequestHandler
    {
        /// <summary>
        /// Handles a Rest request, and returns the Rest response possibly asynchronously.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <returns>A possibly asynchronous RestResponse.</returns>
        ValueTask<RestResponse> HandleRequest(RestRequest request);
    }
}
