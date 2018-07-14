using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Rest Request handler for automatically adding 'self' and 'describedby' links, wherever possible.
    /// </summary>
    public class SelfRequestHandler : IRestRequestHandler
    {
        private readonly RestRequestHandlerDelegate next;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">The next handler delegate in the pipeline.</param>
        public SelfRequestHandler(RestRequestHandlerDelegate next)
        {
            this.next = next;
        }
        /// <summary>
        /// Handles the request by adding 'self' and 'describedby' links.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The decorated result.</returns>
        public async ValueTask<RestResponse> HandleRequest(RestRequest request)
        {
            var resp = await next(request);
            if (resp.UntypedResult is IHasRestValue hrv)
            {
                return resp.Select(rr =>
                    rr.Select(rv =>
                    {
                        if (rv.Value is IHasIdentity hid)
                            rv = rv.AddLink(Link.Create("self", hid.Id));
                        rv = rv.AddLink(Link.Create("describedby", FreeIdentity<TypeDescriptor>.Create(rv.ValueType)));
                        return rv;
                    }));
            }
            else
                return resp;
        }
    }
}
