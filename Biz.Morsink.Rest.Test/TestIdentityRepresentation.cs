using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using System;
using System.ComponentModel.DataAnnotations;

namespace Biz.Morsink.Rest.Test
{
    public class TestIdentityRepresentation : ITypeRepresentation
    {
        public static ITypeRepresentation Instance { get; } = new TestIdentityRepresentation();

        public object GetRepresentable(object rep, Type specific)
            => rep is Representation repr
            ? FreeIdentity<object>.Create(repr.Href)
            : null;

        public Type GetRepresentableType(Type type)
            => type == typeof(Representation) ? typeof(IIdentity) : null;

        public object GetRepresentation(object obj)
            => obj is IIdentity id
            ? new Representation { Href = $"/{id.ForType.Name}/{id.Value}" }
            : null;

        public Type GetRepresentationType(Type type)
            => typeof(IIdentity).IsAssignableFrom(type) ? typeof(Representation) : null;

        public bool IsRepresentable(Type type)
            => GetRepresentationType(type) != null;

        public bool IsRepresentation(Type type)
            => GetRepresentableType(type) != null;

        public class Representation
        {
            [Required]
            public string Href { get; set; }
        }

    }
}
