using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Identity
{
    class AttributeBasedRestIdentityProvider : RestIdentityProvider
    {
        public AttributeBasedRestIdentityProvider(IEnumerable<(RestPathAttribute, Type)> attributes) : base() {
            foreach(var (attr,type) in attributes)
                BuildEntry(attr.ComponentTypes).WithPathAndQueryType(attr.Path, attr.WildcardType).Add();
        }
    }
}
