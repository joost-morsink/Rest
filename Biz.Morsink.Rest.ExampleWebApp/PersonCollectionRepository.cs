using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A repository for Collections of Persons.
    /// </summary>
    public class PersonCollectionRepository 
        : RestRepository<PersonCollection>
        , IRestGet<PersonCollection, NoParameters>
        , IRestPost<PersonCollection, NoParameters, Person, Person>
    {
        private readonly IRestResourceCollection<PersonCollection, Person> resources;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resources">The backing store.</param>
        public PersonCollectionRepository(IRestResourceCollection<PersonCollection, Person> resources)
        {
            this.resources = resources;
        }
        /// <summary>
        /// Get implementation for PersonCollection.
        /// </summary>
        /// <param name="id">The criteria for searching the PersonCollection.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>A Rest response containing the search results.</returns>
        public ValueTask<RestResponse<PersonCollection>> Get(IIdentity<PersonCollection> id, NoParameters parameters)
            => Rest.Value(resources.GetCollection(id)).ToResponseAsync();

        /// <summary>
        /// Post implementation of a Person to a PersonCollection.
        /// </summary>
        /// <param name="target">The identity value for the PersonCollection. Ignored in current implementation.</param>
        /// <param name="parameters">No parameters.</param>
        /// <param name="entity">The entity to put to the backing store.</param>
        /// <returns>An asynchronous Rest response that may contain the posted Person entity.</returns>
        public ValueTask<RestResponse<Person>> Post(IIdentity<PersonCollection> target, NoParameters parameters, Person entity)
        {
            var ent = resources.Post(entity);
            return Rest.Value(ent).ToResponse().WithMetadata(new Location { Address = ent.Id }).ToAsync();
        }
    }
}
