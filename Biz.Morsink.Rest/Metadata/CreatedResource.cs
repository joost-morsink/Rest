using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Metadata
{
    /// <summary>
    /// Metadata to indicate a resource has been created by the request.
    /// </summary>
    public class CreatedResource
    {
        /// <summary>
        /// Gets or sets the location of the created resource.
        /// </summary>
        public IIdentity Address { get; set; }
    }
}
