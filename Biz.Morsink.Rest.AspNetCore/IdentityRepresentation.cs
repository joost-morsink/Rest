using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Type representation for Identity values.
    /// </summary>
    public class IdentityRepresentation : ITypeRepresentation
    {
        private readonly IRestIdentityProvider identityProvider;
        private readonly RestPrefixContainer prefixes;
        private readonly bool useCuries;
        private readonly ICurrentHttpRestConverterAccessor currentHttpRestConverterAccessor;
        protected bool UseCuries => useCuries && currentHttpRestConverterAccessor.CurrentHttpRestConverter.SupportsCuries;

        private class representation
        {
            [Required]
            public string Href { get; set; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identityProvider">The Rest identity provider.</param>
        /// <param name="prefixes">A RestPrefixContainer.</param>
        /// <param name="options">Options for Rest for ASP.Net Core.</param>
        public IdentityRepresentation(IRestIdentityProvider identityProvider, RestPrefixContainer prefixes, IOptions<RestAspNetCoreOptions> options, ICurrentHttpRestConverterAccessor currentHttpRestConverterAccessor)
        {
            this.identityProvider = identityProvider;
            this.prefixes = prefixes;
            useCuries = options.Value.UseCuries;
            this.currentHttpRestConverterAccessor = currentHttpRestConverterAccessor;
            
        }
        /// <summary>
        /// Gets the IIdentity value correspoding to the Href representation.
        /// </summary>
        /// <param name="rep">An Href representation.</param>
        /// <returns></returns>
        public object GetRepresentable(object rep)
            => identityProvider.Parse(((representation)rep).Href, true, prefixes);

        /// <summary>
        /// Gets the Href representation type if the type is an IIdentity.
        /// </summary>
        /// <param name="type">The type to check.</param>
        public Type GetRepresentableType(Type type)
            => type == typeof(representation) ? typeof(IIdentity) : null;

        /// <summary>
        /// Gets an Href representation for IIdentity values.
        /// </summary>
        /// <param name="obj">An object that is supposed to be an IIdentity.</param>
        /// <returns>An Href representation if the specified object is an IIdentity.</returns>
        public object GetRepresentation(object obj)
        {
            var path = identityProvider.ToPath((IIdentity)obj);
            if (UseCuries && prefixes.TryMatch(path, out var prefix))
                path = $"[{prefix.Abbreviation}:{path.Substring(prefix.Prefix.Length)}]";
            return path == null ? null : new representation { Href = path };
        }
        /// <summary>
        /// Gets the Href representation type if the specified type is an IIdentity. 
        /// </summary>
        /// <param name="type">The type to check.</param>
        public Type GetRepresentationType(Type type)
            => typeof(IIdentity).GetTypeInfo().IsAssignableFrom(type) ? typeof(representation) : null;
        /// <summary>
        /// Returns true if the specified type implements or extends IIdentity.
        /// </summary>
        public bool IsRepresentable(Type type)
            => typeof(IIdentity).GetTypeInfo().IsAssignableFrom(type);
        /// <summary>
        /// Returns true if the specified type is the nested Href representation type.
        /// </summary>
        public bool IsRepresentation(Type type)
            => type == typeof(representation);
    }
}
