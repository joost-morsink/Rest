using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// Metadata class for a resource location.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Contains the Location Address of some entity that is somehow related to the request.
        /// </summary>
        public IIdentity Address { get; set; }
    }
}
