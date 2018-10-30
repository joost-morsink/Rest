using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// An interface for the generic manipulation of the HttpContext before serializing a result.
    /// </summary>
    public interface IHttpContextManipulator
    {
        /// <summary>
        /// Manipulates the HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext to manipulate.</param>
        /// <param name="response">The RestResponse that is going to be serialized to the HttpResponse of the context.</param>
        void ManipulateContext(HttpContext context, RestResponse response);
    }
}
