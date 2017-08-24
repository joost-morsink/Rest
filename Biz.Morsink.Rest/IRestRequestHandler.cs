using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public delegate ValueTask<RestResponse> RestRequestHandlerDelegate(RestRequest request);
    public interface IRestRequestHandler
    {
        ValueTask<RestResponse> HandleRequest(RestRequest request);
    }
}
