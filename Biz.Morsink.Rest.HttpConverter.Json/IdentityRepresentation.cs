using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json
{
    /// <summary>
    /// Type representation for Identity values.
    /// </summary>
    public class IdentityRepresentation : ITypeRepresentation
    {
        private readonly IRestIdentityProvider identityProvider;

        private class representation
        {
            public string Href { get; set; }
        }

        public IdentityRepresentation(IRestIdentityProvider identityProvider)
        {
            this.identityProvider = identityProvider;
        }
        public object GetRepresentable(object rep)
            => identityProvider.Parse(((representation)rep).Href, true);


        public Type GetRepresentableType(Type type)
            => type == typeof(representation) ? typeof(IIdentity) : null;

        public object GetRepresentation(object obj)
        {
            var path = identityProvider.ToPath((IIdentity)obj);
            return path == null ? null : new representation { Href = path };
        }

        public Type GetRepresentationType(Type type)
            => typeof(IIdentity).GetTypeInfo().IsAssignableFrom(type) ? typeof(representation) : null;

        public bool IsRepresentable(Type type)
            => typeof(IIdentity).GetTypeInfo().IsAssignableFrom(type);

        public bool IsRepresentation(Type type)
            => type == typeof(representation);
    }
}
