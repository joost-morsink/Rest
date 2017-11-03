using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;
using Biz.Morsink.Rest.AspNetCore.Identity;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A repository for Collections of Persons.
    /// </summary>
    [RestPath("/person?*", WildcardType = typeof(PersonCollection.Parameters))]
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
        public async ValueTask<RestResponse<PersonCollection>> Get(IIdentity<PersonCollection> id, NoParameters parameters)
            => Rest.Value(await resources.GetCollection(id)).ToResponse();

        /// <summary>
        /// Post implementation of a Person to a PersonCollection.
        /// </summary>
        /// <param name="target">The identity value for the PersonCollection. Ignored in current implementation.</param>
        /// <param name="parameters">No parameters.</param>
        /// <param name="entity">The entity to put to the backing store.</param>
        /// <returns>An asynchronous Rest response that may contain the posted Person entity.</returns>
        public async ValueTask<RestResponse<Person>> Post(IIdentity<PersonCollection> target, NoParameters parameters, Person entity)
        {
            var ent = await resources.Post(entity);
            return Rest.Value(ent).ToResponse().WithMetadata(new CreatedResource { Address = ent.Id });
        }
    }
}
