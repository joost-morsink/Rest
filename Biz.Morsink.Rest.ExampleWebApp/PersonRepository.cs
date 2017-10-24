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
        private readonly IRestResourceCollection<PersonCollection, Person> resources;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PersonRepository(IRestResourceCollection<PersonCollection, Person> resources)
        {
            this.resources = resources;
        }

        /// <summary>
        /// Get implementation for Person.
        /// </summary>
        /// <param name="id">The identity value for the Person.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest response that may contain a Person entity.</returns>
        public ValueTask<RestResponse<Person>> Get(IIdentity<Person> id, NoParameters parameters)
        {
            var p = resources.Get(id);
            if (p != null)
                return Rest.Value(p).ToResponseAsync();
            else
                return RestResult.NotFound<Person>().ToResponseAsync();
        }
    }
}
