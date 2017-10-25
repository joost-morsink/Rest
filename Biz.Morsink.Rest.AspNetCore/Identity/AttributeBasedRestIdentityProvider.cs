using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    class AttributeBasedRestIdentityProvider : RestIdentityProvider
    {
        public AttributeBasedRestIdentityProvider() : base()
        {

        }

        public void Initialize(IEnumerable<(RestPathAttribute, Type)> attributes)
        {
            foreach (var (attr, type) in attributes)
            {
                if (attr.WildcardType != null)
                    BuildEntry(attr.ComponentTypes).WithPathAndQueryType(attr.Path, attr.WildcardType).Add();
                else
                    BuildEntry(attr.ComponentTypes).WithPath(attr.Path).Add();
            }
        }
        public void Initialize(IServiceCollection serviceCollection)
            => Initialize(from desc in serviceCollection
                          where typeof(IRestRepository).IsAssignableFrom(desc.ServiceType)
                          from attr in desc.ImplementationType.GetTypeInfo().GetCustomAttributes<RestPathAttribute>()
                          select (attr, desc.ImplementationType));

    }
}
