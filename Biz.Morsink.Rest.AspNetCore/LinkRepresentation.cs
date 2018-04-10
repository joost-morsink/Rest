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
    public class LinkRepresentation : ITypeRepresentation
    {
        /// <summary>
        /// The representation class.
        /// </summary>
        private class Representation
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

        public object GetRepresentable(object rep)
        {
            throw new NotSupportedException();
        }

        public Type GetRepresentableType(Type type)
            => type == typeof(Representation) ? typeof(Link) : null;

        public object GetRepresentation(object obj)
           => new Representation((Link)obj);

        public Type GetRepresentationType(Type type)
            => typeof(Link).IsAssignableFrom(type) ? typeof(Representation) : null;

        public bool IsRepresentable(Type type)
            => typeof(Link).IsAssignableFrom(type);

        public bool IsRepresentation(Type type)
            => typeof(Representation) == type;
    }
}
