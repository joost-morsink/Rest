using Biz.Morsink.Rest.Schema;
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
            BuildEntry(typeof(TypeDescriptor)).WithPath("/schema/*").Add();
        }

        internal void Initialize(IEnumerable<IRestRepository> repositories)
        {
            foreach (var repo in repositories)
            {
                foreach(var attr in repo.GetType().GetTypeInfo().GetCustomAttributes<RestPathAttribute>())
                {
                    var compTypes = attr.ComponentTypes ?? new Type[] { repo.EntityType };
                    if (attr.WildcardType != null)
                        BuildEntry(compTypes).WithPathAndQueryType(attr.Path, attr.WildcardType).Add();
                    else
                        BuildEntry(compTypes).WithPath(attr.Path).Add();
                }
            }
        }
    }
}
