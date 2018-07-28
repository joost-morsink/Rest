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
    public class IdentityRepresentation : SimpleTypeRepresentation<IIdentity, IdentityRepresentation.Representation>
    {
        private readonly IRestIdentityProvider identityProvider;
        private readonly IRestPrefixContainerAccessor prefixContainerAccessor;
        private readonly bool useCuries;
        private readonly ICurrentHttpRestConverterAccessor currentHttpRestConverterAccessor;
        protected bool UseCuries => useCuries && currentHttpRestConverterAccessor.CurrentHttpRestConverter.SupportsCuries;

        public class Representation
        {
            [Required]
            public string Href { get; set; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identityProvider">The Rest identity provider.</param>
        /// <param name="prefixContainerAccessor">A RestPrefixContainer accessor.</param>
        /// <param name="options">Options for Rest for ASP.Net Core.</param>
        public IdentityRepresentation(IRestIdentityProvider identityProvider, IRestPrefixContainerAccessor prefixContainerAccessor, IOptions<RestAspNetCoreOptions> options, ICurrentHttpRestConverterAccessor currentHttpRestConverterAccessor)
        {
            this.identityProvider = identityProvider;
            this.prefixContainerAccessor = prefixContainerAccessor;
            useCuries = options.Value.UseCuries;
            this.currentHttpRestConverterAccessor = currentHttpRestConverterAccessor;
            
        }

        public override Representation GetRepresentation(IIdentity item)
        {
            var path = identityProvider.ToPath(item);
            if (UseCuries && prefixContainerAccessor.RestPrefixContainer.TryMatch(path, out var prefix))
                path = $"[{prefix.Abbreviation}:{path.Substring(prefix.Prefix.Length)}]";
            return path == null ? null : new Representation { Href = path };
        }

        public override IIdentity GetRepresentable(Representation representation)
            => identityProvider.Parse(representation.Href, true, prefixContainerAccessor.RestPrefixContainer);
    }
}
