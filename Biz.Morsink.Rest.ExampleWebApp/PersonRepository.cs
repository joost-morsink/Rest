using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Biz.Morsink.Rest.Metadata;
using System.Threading;
using Biz.Morsink.Rest.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A repository for Person entities.
    /// </summary>
    [RestDocumentation("Repository for persons.")]
    public class PersonRepository
    {
        public class DelayParameter
        {
            [RestDocumentation("Delay in seconds. For testing purposes.")]
            public int Delay { get; set; }
        }
        private static readonly ResponseCaching CACHING = new ResponseCaching
        {
            CacheAllowed = false,
            StoreAllowed = true,
            CachePrivate = true,
            Validity = TimeSpan.FromMinutes(10.0)
        };
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
        [RestDelete]
        [RestDocumentation("Deletes a person from the person store.")]
        public async ValueTask<RestResponse<object>> Delete(IIdentity<Person> target)
            => await resources.Delete(target)
                ? Rest.Value(new object()).ToResponse()
                : RestResult.NotFound<object>().ToResponse();

        /// <summary>
        /// Get implementation for Person.
        /// </summary>
        /// <param name="id">The identity value for the Person.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest response that may contain a Person entity.</returns>
        [RestGet]
        [RestDocumentation("Gets a person from the person store.")]
        public async ValueTask<RestResponse<Person>> Get(IIdentity<Person> id)
        {
            var p = await resources.Get(id);
            if (p != null)
                return Rest.Value(p).ToResponse().WithMetadata(CACHING);
            else
                return RestResult.NotFound<Person>().ToResponse().WithMetadata(CACHING);
        }
        /// <summary>
        /// Put implementation for Person.
        /// </summary>
        /// <param name="target">The identity value for the Person.</param>
        /// <param name="parameters">No parameters.</param>
        /// <param name="entity">The entity to put to the backing store.</param>
        /// <returns>An asynchronous Rest response that may contain the updated Person entity.</returns>
        [RestPut]
        [RestDocumentation("Upserts a person into the person store.")]
        public async ValueTask<RestResponse<Person>> Put(IIdentity<Person> target, [RestBody]Person entity)
            => entity.Id == null || target.Equals(entity.Id)
                ? Rest.Value(await resources.Put(entity)).ToResponse()
                : RestResult.BadRequest<Person>(new object()).ToResponse();

        /// <summary>
        /// Get implementation for PersonCollection.
        /// </summary>
        /// <param name="id">The criteria for searching the PersonCollection.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>A Rest response containing the search results.</returns>
        [RestGet]
        [RestDocumentation("Searches for persons in the person store.")]
        [RestParameter(typeof(SimpleSearchParameters))]
        [RestParameter(typeof(CollectionParameters))]
        public async ValueTask<RestResponse<PersonCollection>> Get(IIdentity<PersonCollection> id)
        {
            var collectionParameters = id.Provider.GetConverter(typeof(PersonCollection), false).Convert(id.Value).To<CollectionParameters>();
            if (collectionParameters != null && (collectionParameters.Limit <= 0 || collectionParameters.Skip < 0))
                return RestResult.BadRequest<PersonCollection>(new object()).ToResponse();
            return Rest.Collection(await resources.GetCollection(id)).ToResponse();
        }
        /// <summary>
        /// Post implementation of a Person to a PersonCollection.
        /// </summary>
        /// <param name="target">The identity value for the PersonCollection. Ignored in current implementation.</param>
        /// <param name="parameters">No parameters.</param>
        /// <param name="entity">The entity to put to the backing store.</param>
        /// <returns>An asynchronous Rest response that may contain the posted Person entity.</returns>
        [RestPost]
        [RestDocumentation("Inserts a person into the person store.")]
        [RestMetaDataOut(typeof(CreatedResource))]
        public async ValueTask<RestResponse<Person>> Post(IIdentity<PersonCollection> target, DelayParameter parameters, Person entity, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(parameters.Delay), cancellationToken);

            var ent = await resources.Post(entity);
            return Rest.Value(ent).ToResponse().WithMetadata(new CreatedResource { Address = ent.Id });
        }

        public class Structure : IRestStructure
        {
            public void RegisterComponents(IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            {
                serviceCollection.AddAttributedRestRepository<PersonRepository>(lifetime)
                    .AddSingleton<IRestResourceCollection<PersonCollection, Person>, PersonSource>()
                    .AddRestPathMapping<Person>("/person/*")
                    .AddRestPathMapping<PersonCollection>("/person?*", null, typeof(SimpleSearchParameters), typeof(CollectionParameters))
                    .AddScoped<IDynamicLinkProvider<PersonCollection>, PersonCollectionLinks>()
                    ;
            }
        }
    }
}