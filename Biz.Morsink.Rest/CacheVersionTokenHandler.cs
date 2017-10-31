using Biz.Morsink.Rest.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Rest Request handler for adding VersionTokens for types that have a ITokenProvider&lt;T&gt;.
    /// </summary>
    public class CacheVersionTokenHandler : IRestRequestHandler
    {
        private readonly RestRequestHandlerDelegate next;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next Rest request handler in the pipeline.</param>
        /// <param name="serviceProvider">A service provider for resolving token providers.</param>
        public CacheVersionTokenHandler(RestRequestHandlerDelegate next, IServiceProvider serviceProvider)
        {
            this.next = next;
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Handles the Rest Request.
        /// </summary>
        /// <param name="request">A Rest request.</param>
        /// <returns>The Rest response.</returns>
        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            if (request.Capability == "GET")
            {
                var resp = await next(request);
                if (resp.IsSuccess)
                {
                    var restValue = resp.UntypedResult.AsSuccess().RestValue;
                    var tokenProvider = (ITokenProvider)serviceProvider.GetService(typeof(ITokenProvider<>).MakeGenericType(restValue.ValueType));
                    if (tokenProvider != null)
                        return resp.AddMetadata(new VersionToken { Token = tokenProvider.GetTokenFor(restValue.Value) });
                    else
                        return resp;
                }
                else
                    return resp;
            }
            else
                return await next(request);
        }
    }
}
