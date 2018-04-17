using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    class DefaultAspRestIdentityProvider : RestIdentityProvider
    {
        public DefaultAspRestIdentityProvider(string localPrefix = null) : base(localPrefix)
        {
            BuildEntry(typeof(TypeDescriptor)).WithPath("/schema/*").Add();
        }

        internal void Initialize(IEnumerable<IRestRepository> repositories, IEnumerable<IRestPathMapping> pathMappings)
        {
            foreach (var mapping in pathMappings)
            {
                if (mapping.WildcardTypes != null && mapping.WildcardTypes.Length > 0)
                    BuildEntry(mapping.ComponentTypes).WithPathAndQueryType(mapping.RestPath, mapping.WildcardTypes).Add();
                else
                    BuildEntry(mapping.ComponentTypes).WithPath(mapping.RestPath).Add();
            }
        }
    }
}
