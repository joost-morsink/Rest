using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{

    public class LinkRepresentation : ITypeRepresentation
    {
        private class Representation
        {
            public Representation()
            {

            }
            public Representation(Link lnk)
            {
                Capability = lnk.GetCapabilityString();
                Target = lnk.Target;
                RelType = lnk.RelType;
                Parameters = lnk.Parameters;
            }
            [Required]
            public string Capability { get; set; }
            [Required]
            public IIdentity Target { get; set; }
            [Required]
            public string RelType { get; set; }
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
