using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Type representation for Link.
    /// </summary>
    public class LinkRepresentation : SimpleTypeRepresentation<Link,LinkRepresentation.Representation>
    {
        /// <summary>
        /// The representation class.
        /// </summary>
        public class Representation
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public Representation()
            {

            }
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="link">The link to represent.</param>
            public Representation(Link link)
            {
                Capability = link.GetCapabilityString();
                Target = link.Target;
                RelType = link.RelType;
                Parameters = link.Parameters;
            }
            /// <summary>
            /// The capability for the link.
            /// </summary>
            [Required]
            public string Capability { get; set; }
            /// <summary>
            /// The target of the link.
            /// </summary>
            [Required]
            public IIdentity Target { get; set; }
            /// <summary>
            /// The 'reltype' for the link.
            /// </summary>
            [Required]
            public string RelType { get; set; }
            /// <summary>
            /// Optional parameters for the link.
            /// </summary>
            public object Parameters { get; set; }
        }

        public override Representation GetRepresentation(Link item)
            => new Representation(item);

        public override Link GetRepresentable(Representation representation)
            => Link.Create(representation.RelType, representation.Target, representation.Parameters);
    }
}
