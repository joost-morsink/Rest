using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class PersonRepository : RestRepository<Person>, IRestGet<Person, NoParameters>
    {
        private Dictionary<string, Person> data = new Dictionary<string, Person>
        {
            ["1"] = new Person(/*FreeIdentity<Person>.Create(1),*/ "Joost", "Morsink", 38)
        };
        public PersonRepository() { }

        public ValueTask<RestResponse<Person>> Get(IIdentity<Person> id, NoParameters parameters)
        {
            var key = id.Value?.ToString();
            if (key != null && data.TryGetValue(key, out var p))
                return Rest.Value(p).ToResponseAsync();
            else
                return RestResult.NotFound<Person>().ToResponseAsync();
        }
    }
}
