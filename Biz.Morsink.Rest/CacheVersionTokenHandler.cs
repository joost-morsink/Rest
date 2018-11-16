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
        private readonly ITokenProviderFactory tokenProviderFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next Rest request handler in the pipeline.</param>
        /// <param name="serviceProvider">A service provider for resolving token providers.</param>
        public CacheVersionTokenHandler(RestRequestHandlerDelegate next, ITokenProviderFactory tokenProviderFactory)
        {
            this.next = next;
            this.tokenProviderFactory = tokenProviderFactory;
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
                    var tokenProvider = tokenProviderFactory.GetTokenProvider(restValue.ValueType);
                    if (tokenProvider != null)
                    {
                        var token = tokenProvider.GetTokenFor(restValue.Value);
                        if (request.Metadata.TryGet<TokenMatching>(out var tokenMatching) && !tokenMatching.Matches && tokenMatching.Tokens.Contains(token))
                            return resp.Select(r => r.MakeNotNecessary()).AddMetadata(token);
                        else
                            return resp.AddMetadata(token);
                    }
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
