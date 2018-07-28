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
    [RestDocumentation("Repository for persons. Version 1.")]
    public class PersonRepository
    {
        private static IIdentity<Person> Convert(IIdentity<PersonV2> id)
        {
            return id.Provider.Creator<Person>().Create(id.Value);
        }
        private static IIdentity<PersonV2> Convert(IIdentity<Person> id)
        {
            return id.Provider.Creator<PersonV2>().Create(id.Value);
        }
        private static IIdentity<PersonCollection> Convert(IIdentity<PersonV2Collection> id)
        {
            return id.Provider.Creator<PersonCollection>().Create(id.Value);
        }
        private static IIdentity<PersonV2Collection> Convert(IIdentity<PersonCollection> id)
        {
            return id.Provider.Creator<PersonV2Collection>().Create(id.Value);
        }
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
        private readonly IRestRepository<PersonV2> repository;
        private readonly IRestRepository<PersonV2Collection> collectionRepository;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PersonRepository(IRestRepository<PersonV2> repository, IRestRepository<PersonV2Collection> collectionRepository)
        {
            this.repository = repository;
            this.collectionRepository = collectionRepository;
        }
        /// <summary>
        /// Delete imnplementation for Person.
        /// </summary>
        /// <param name="target">The identity value for the Person.</param>
        /// <param name="parameters">No parameters.</param>
        /// <returns>An asynchronous Rest response that indicates whether the deletion succeeded or failed.</returns>
        [RestDelete]
        [RestDocumentation("Deletes a person from the person store.")]
        public ValueTask<RestResponse<object>> Delete(IIdentity<Person> target)
            => repository.GetCapability<IRestDelete<PersonV2, Empty>>().Delete(Convert(target), new Empty(), default);

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
            var resp = await repository.GetCapability<IRestGet<PersonV2, Empty>>().Get(Convert(id), new Empty(), default);
            return resp.Select(r => r.Select(v => new RestValue<Person>(v.Value.ToV1(), v.Links, v.Embeddings)));
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
        {
            var resp = await repository.GetCapability<IRestPut<PersonV2, Empty>>().Put(Convert(target), new Empty(), PersonV2.Create(entity), default);
            return resp.Select(r => r.Select(v => new RestValue<Person>(v.Value.ToV1(), v.Links, v.Embeddings)));
        }
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
            var resp = await collectionRepository.GetCapability<IRestGet<PersonV2Collection, Empty>>().Get(Convert(id), new Empty(), default);
            return resp.Select(r => r.Select(v => new RestValue<PersonCollection>(v.Value.ToV1(), v.Links, v.Embeddings)));
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
            var resp = await collectionRepository.GetCapability<IRestPost<PersonV2Collection, DelayParameter, PersonV2, PersonV2>>()
                .Post(Convert(target), parameters, PersonV2.Create(entity), cancellationToken);
            return resp.Select(r => r.Select(v => new RestValue<Person>(v.Value.ToV1(), v.Links, v.Embeddings)));
        }

    }
    /// <summary>
    /// A repository for Person entities.
    /// </summary>
    [RestDocumentation("Repository for persons. Version 2.")]
    public class PersonV2Repository
    {
        private static readonly ResponseCaching CACHING = new ResponseCaching
        {
            CacheAllowed = false,
            StoreAllowed = true,
            CachePrivate = true,
            Validity = TimeSpan.FromMinutes(10.0)
        };
        private readonly IRestResourceCollection<PersonV2Collection, PersonV2> resources;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resources">The backing store.</param>
        public PersonV2Repository(IRestResourceCollection<PersonV2Collection, PersonV2> resources)
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
        public async ValueTask<RestResponse<object>> Delete(IIdentity<PersonV2> target)
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
        public async ValueTask<RestResponse<PersonV2>> Get(IIdentity<PersonV2> id)
        {
            var p = await resources.Get(id);
            if (p != null)
                return Rest.Value(p).ToResponse().WithMetadata(CACHING);
            else
                return RestResult.NotFound<PersonV2>().ToResponse().WithMetadata(CACHING);
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
        public async ValueTask<RestResponse<PersonV2>> Put(IIdentity<PersonV2> target, [RestBody]PersonV2 entity)
            => entity.Id == null || target.Equals(entity.Id)
                ? Rest.Value(await resources.Put(entity)).ToResponse()
                : RestResult.BadRequest<PersonV2>(new object()).ToResponse();

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
        public async ValueTask<RestResponse<PersonV2Collection>> Get(IIdentity<PersonV2Collection> id)
        {
            var collectionParameters = id.Provider.GetConverter(typeof(PersonV2Collection), false).Convert(id.Value).To<CollectionParameters>();
            if (collectionParameters != null && (collectionParameters.Limit <= 0 || collectionParameters.Skip < 0))
                return RestResult.BadRequest<PersonV2Collection>(new object()).ToResponse();
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
        public async ValueTask<RestResponse<PersonV2>> Post(IIdentity<PersonV2Collection> target, PersonRepository.DelayParameter parameters, PersonV2 entity, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(parameters.Delay), cancellationToken);

            var ent = await resources.Post(entity);
            return Rest.Value(ent).ToResponse().WithMetadata(new CreatedResource { Address = ent.Id });
        }

        public class Structure : IRestStructure
        {
            public void RegisterComponents(IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            {
                var wildcards = new[] { typeof(SimpleSearchParameters), typeof(CollectionParameters) };
                serviceCollection.AddAttributedRestRepository<PersonV2Repository>(lifetime)
                    .AddAttributedRestRepository<PersonRepository>(lifetime)
                    .AddSingleton<IRestResourceCollection<PersonV2Collection, PersonV2>, PersonSource>()
                    .OnRestPath("/person/*", bld =>
                        bld.ForVersion(1).Add<Person>()
                        .ForVersion(2).Add<PersonV2>())
                    .OnRestPath("/person?*", bld => 
                        bld.ForVersion(1).Add<PersonCollection>(wildcards)
                        .ForVersion(2).Add<PersonV2Collection>(wildcards))
                    .AddScoped<IDynamicLinkProvider<PersonV2Collection>, PersonCollectionLinks>()
                    ;
            }
        }
    }
}