using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Metadata;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    public class OptionsRequestHandler : IRestRequestHandler
    {
        private readonly RestRequestHandlerDelegate next;
        private readonly IServiceProvider locator;
        private readonly TypeDescriptorCreator typeDescriptorCreator;

        public OptionsRequestHandler(RestRequestHandlerDelegate next, IServiceProvider locator, TypeDescriptorCreator typeDescriptorCreator)
        {
            this.next = next;
            this.locator = locator;
            this.typeDescriptorCreator = typeDescriptorCreator;
        }

        public ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            if (request.Capability.ToUpperInvariant() == "OPTIONS")
            {
                var repo = (IRestRepository)locator.GetService(typeof(IRestRepository<>).MakeGenericType(request.Address.ForType));
                var idprov = (IRestIdentityProvider)locator.GetService(typeof(IRestIdentityProvider));
                var res = Utilities.MakeCapabilities(idprov, repo, typeDescriptorCreator);
                return Rest.Value(res).ToResponse().AddMetadata(new Capabilities(res.Keys.Concat(new[] { "OPTIONS" }))).ToAsync();
            }
            else
                return next(request);
        }
    }
}
