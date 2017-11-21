using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Biz.Morsink.Rest.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class BlogRepository
    {
        public static ConcurrentDictionary<string, Blog> data;
        public BlogRepository()
        {
            var blogId = FreeIdentity<Blog>.Create("1");
            data = data ?? new ConcurrentDictionary<string, Blog>(new[] { new KeyValuePair<string, Blog>("1", new Blog { Id = blogId, Name = "Joost's blog", Owner = FreeIdentity<Person>.Create(1) }) });
        }

        [RestGet]
        public RestResult<Blog> Get(IIdentity<Blog> blogId)
        {
            var id = blogId.Value.ToString();
            if (data.TryGetValue(id, out var blog))
                return Rest.Value(blog).ToResult();
            else
                return RestResult.NotFound<Blog>();
        }
        [RestGet]
        public RestValue<BlogCollection> Get(IIdentity<BlogCollection> collId)
        {
            return Rest.Value(new BlogCollection(collId, data.Values, data.Count, null, 0));
        }

        public class Structure : IRestStructure
        {
            void IRestStructure.RegisterComponents(IServiceCollection serviceCollection, ServiceLifetime lifetime)
            {
                serviceCollection.AddAttributedRestRepository<BlogRepository>()
                    .AddRestPathMapping<Blog>("/blog/*")
                    .AddRestPathMapping<BlogCollection>("/blog?*");
            }
        }
    }
}
