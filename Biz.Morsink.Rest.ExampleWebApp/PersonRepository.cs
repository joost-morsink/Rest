using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Identity;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A repository for Person entities.
    /// </summary>
    [RestPath("/person/*")]
    public class PersonRepository 
        : RestRepository<Person>
        , IRestGet<Person, NoParameters>
        , IRestPut<Person, NoParameters>
        , IRestDelete<Person, NoParameters>
    {
        private readonly IRestResourceCollection<PersonCollection, Person> resources;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resources">The backing store.</param>
        public PersonRepository(IRestResourceCollection<PersonCollection, Person> resources)
        {
            this.resources = resources;
        }
        /// <summary>
        /// Delete imnplementation for Person.
        /// </summary>
        /// <param name="target">The identity value for the Person.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest response that indicates whether the deletion succeeded or failed.</returns>
        public ValueTask<RestResponse<object>> Delete(IIdentity<Person> target, NoParameters parameters)
            => resources.Delete(target)
                ? Rest.Value(new object()).ToResponseAsync()
                : RestResult.NotFound<object>().ToResponseAsync();

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
        /// <summary>
        /// Put implementation for Person.
        /// </summary>
        /// <param name="target">The identity value for the Person.</param>
        /// <param name="parameters">No parameters.</param>
        /// <param name="entity">The entity to put to the backing store.</param>
        /// <returns>An asynchronous Rest response that may contain the updated Person entity.</returns>
        public ValueTask<RestResponse<Person>> Put(IIdentity<Person> target, NoParameters parameters, Person entity)
            => entity.Id == null || target.Equals(entity.Id)
                ? Rest.Value(resources.Put(entity)).ToResponseAsync()
                : RestResult.BadRequest<Person>(new object()).ToResponseAsync();
    }
}