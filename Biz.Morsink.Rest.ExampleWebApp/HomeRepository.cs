using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class HomeRepository : RestRepository<Home>, IRestGet<Home, NoParameters>
    {
        public ValueTask<RestResponse<Home>> Get(IIdentity<Home> id, NoParameters parameters)
        {
            return Rest.ValueBuilder(Home.Instance).WithLink(Link.Create("admin", FreeIdentity<Person>.Create(1))).BuildResponseAsync();
        }
    }
}
