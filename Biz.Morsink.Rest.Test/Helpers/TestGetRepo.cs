using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.Test.Helpers
{
    public class TestGetRepo : RestRepository<Person>, IRestGet<Person>
    {
        public ValueTask<RestResult<Person>> Get(IIdentity<Person> id)
        {
            if (id.Value?.ToString() == "1")
            {
                var p = new Person { FirstName = "Joost", LastName = "Morsink", Age = 37 };
                return Rest.Value(p).ToResultAsync();
            }
            else
                return RestResult.NotFound<Person>().ToAsync();
        }
    }
}
