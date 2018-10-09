using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Metadata;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
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
        private readonly ITypeDescriptorCreator typeDescriptorCreator;

        public OptionsRequestHandler(RestRequestHandlerDelegate next, IServiceProvider locator, ITypeDescriptorCreator typeDescriptorCreator)
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
                var resp = Rest.Value(res).ToResponse().AddMetadata(new Capabilities(res.Keys.Append("OPTIONS")));
                if (request.Metadata.TryGet(out Versioning ver))
                    resp = resp.AddMetadata(ver);
                return resp.ToAsync();
            }
            else
                return next(request);
        }
    }
}
