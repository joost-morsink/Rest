using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore
{
    public interface IHttpRestConverter
    {
        bool Applies(HttpContext context);

        RestRequest ManipulateRequest(RestRequest req, HttpContext context);

        object ParseBody(Type t, byte[] body);

        Task SerializeResponse(RestResponse response, HttpContext context);
    }
}
