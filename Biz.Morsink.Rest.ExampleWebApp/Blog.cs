using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class Ownable
    {
        public IIdentity<Person> Owner { get; set; }
    }
    public class Blog : Ownable, IHasIdentity<Blog> 
    {
        public IIdentity<Blog> Id { get; set; }
        public string Name { get; set; }

        IIdentity IHasIdentity.Id => Id;
    }
    public class BlogCollection : RestCollection<Blog>
    {
        public BlogCollection(IIdentity<BlogCollection> id, IEnumerable<Blog> items, int count, int? limit, int skip) : base(id, items, count, limit, skip) { }
    }
}
