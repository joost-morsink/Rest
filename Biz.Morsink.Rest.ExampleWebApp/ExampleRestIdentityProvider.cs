using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class ExampleRestIdentityProvider : RestIdentityProvider
    {
        public ExampleRestIdentityProvider()
        {
            BuildEntry(typeof(TypeDescriptor)).WithPath("/schema/*").Add();
            BuildEntry(typeof(Person)).WithPath("/person/*").Add();
            BuildEntry(typeof(Home)).WithPath("/?*").Add();
        }
    }
}
