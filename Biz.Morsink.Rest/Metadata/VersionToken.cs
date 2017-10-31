using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// A class containing a version token.
    /// This token may be used to revalidate a resource server-side or to conditionally execute requests.
    /// </summary>
    public class VersionToken
    {
        /// <summary>
        /// A token identifying the version of the resource. 
        /// This token may be used to revalidate a resource server-side or to conditionally execute requests.
        /// </summary>
        public string Token { get; set; }
    }
}
