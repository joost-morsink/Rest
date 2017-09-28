using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A repository for Person entities.
    /// </summary>
    public class PersonRepository : RestRepository<Person>, IRestGet<Person, NoParameters>
    {
        /// <summary>
        /// A dictionary containing all the persons in this repository.
        /// </summary>
        private Dictionary<string, Person> data = new Dictionary<string, Person>
        {
            ["1"] = new Person(FreeIdentity<Person>.Create(1), "Joost", "Morsink", 38)
        };
        /// <summary>
        /// Constructor.
        /// </summary>
        public PersonRepository() { }

        /// <summary>
        /// Get implementation for Person.
        /// </summary>
        /// <param name="id">The identity value for the Person.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest response that may contain a Person entity.</returns>
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
