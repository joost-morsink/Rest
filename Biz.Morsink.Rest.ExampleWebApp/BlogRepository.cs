using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class BlogRepository
    {
        [RestGet]
        public ValueTask<RestResponse<Blog>> Get(IIdentity<Blog> blogId)
        {
            if (blogId.Value.ToString() == "1")
                return Rest.Value(new Blog { Id = blogId, Name = "Joost's blog", Owner = blogId.Provider.Creator<Person>().Create(1) }).ToResponseAsync();
            else
                return RestResult.NotFound<Blog>().ToResponseAsync();
        }
    }
}
