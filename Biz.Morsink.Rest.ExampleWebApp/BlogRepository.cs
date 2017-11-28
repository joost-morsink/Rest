using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Biz.Morsink.Rest.AspNetCore.Identity;
using System.Collections.Concurrent;
using System.Threading;
using Biz.Morsink.Rest.Metadata;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class BlogRepository
    {
        private static int counter = 0;
        private static ConcurrentDictionary<string, Blog> data;
        private string next() => Interlocked.Increment(ref counter).ToString();
        public BlogRepository()
        {
            var id = next();
            var blogId = FreeIdentity<Blog>.Create(id);
            data = data ?? new ConcurrentDictionary<string, Blog>(new[] { new KeyValuePair<string, Blog>(id, new Blog { Id = blogId, Name = "Joost's blog", Owner = FreeIdentity<Person>.Create(1) }) });
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
        [RestPost]
        public RestResponse<Blog> Post(IIdentity<BlogCollection> collId, [RestBody] Blog blog)
        {
            var id = next();
            blog.Id = FreeIdentity<Blog>.Create(id);
            data[id] = blog;
            return Rest.Value(blog).ToResponse().WithMetadata(new CreatedResource { Address = blog.Id });
        }
        [RestPut]
        public RestResult<Blog> Put(IIdentity<Blog> id, Empty empty, Blog blog)
        {
            if (blog.Id == null)
                blog.Id = id;
            else if (!blog.Id.Equals(id))
                return RestResult.BadRequest<Blog>("Ids do not match.");
            return Rest.Value(data.AddOrUpdate(id.ComponentValue.ToString(), blog, (_, __) => blog)).ToResult();
        }
        [RestDelete]
        public RestResult<object> Delete(IIdentity<Blog> id)
        {
            if (data.TryRemove(id.ComponentValue.ToString(), out var blog))
                return Rest.Value(new object()).ToResult();
            else
                return RestResult.NotFound<object>();
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
