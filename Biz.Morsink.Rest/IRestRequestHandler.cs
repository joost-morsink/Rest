using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public interface IRestRequestHandler
    {
        ValueTask<IRestResult> HandleRequest(RestRequest request);
    }
}
