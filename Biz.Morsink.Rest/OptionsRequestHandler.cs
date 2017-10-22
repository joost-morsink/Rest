using Biz.Morsink.Rest.Metadata;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
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

        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            if (request.Capability.ToUpperInvariant() == "OPTIONS")
            {
                var repo = (IRestRepository)locator.GetService(typeof(IRestRepository<>).MakeGenericType(request.Address.ForType));
                var res = new RestCapabilities(repo, typeDescriptorCreator);
                return Rest.Value(res).ToResponse().AddMetadata(new Capabilities(res.Keys.Concat(new[] { "OPTIONS" })));
            }
            else
                return await next(request);
        }
    }
}
