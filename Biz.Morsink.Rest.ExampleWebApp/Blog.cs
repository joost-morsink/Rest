using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class Blog : IHasIdentity<Blog>
    {
        public IIdentity<Blog> Id { get; set; }
        public string Name { get; set; }
        public IIdentity<Person> Owner { get; set; }

        IIdentity IHasIdentity.Id => Id;
    }
}
