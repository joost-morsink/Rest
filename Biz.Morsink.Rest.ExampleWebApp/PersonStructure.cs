using Biz.Morsink.Rest.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using System.Collections.Concurrent;
using System.Threading;
using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Metadata;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class PersonStructure : AbstractRestCollectionStructure<PersonCollection, Person>
    {
        private int counter = 1;

        public class PersonCollectionRepository : CollectionRepository
        {
            public class DelayParameter
            {
                public int Delay { get; set; }
            }
            public PersonCollectionRepository(PersonStructure structure) : base(structure) { }

            protected override void RegisterCapabilities()
            {
                Register(new Get(this));
                Register(new Post(this));
            }
            public new class Post : IRestPost<PersonCollection, DelayParameter, Person, Person>
            {
                private readonly PersonCollectionRepository repo;

                public Post(PersonCollectionRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<Person>> IRestPost<PersonCollection, DelayParameter, Person, Person>.Post(IIdentity<PersonCollection> target, DelayParameter parameters, Person entity, CancellationToken cancellationToken)
                {
                    await Task.Delay(TimeSpan.FromSeconds(parameters.Delay));
                    var res = await repo.Source.Post(entity);
                    if (res == null)
                        return RestResult.BadRequest<Person>(new object()).ToResponse();
                    else
                        return Rest.Value(res).ToResponse().WithMetadata(new CreatedResource { Address = res.Id });
                }
            }
        }

        private ConcurrentDictionary<string, Person> data = new ConcurrentDictionary<string, Person>
        {
            ["1"] = new Person("Joost", "Morsink", 38, FreeIdentity<Person>.Create(1))
        };
        private readonly IRestIdentityProvider idProv;

        public PersonStructure(IRestIdentityProvider idProv)
        {
            this.idProv = idProv;
        }

        public override Task<bool> Delete(IIdentity<Person> entityId)
            => Task.FromResult(data.TryRemove(entityId.Value?.ToString(), out var _));


        public override Task<Person> Get(IIdentity<Person> entityId)
            => Task.FromResult(data.TryGetValue(entityId.Value?.ToString(), out var p) ? p : null);

        public override Task<PersonCollection> GetCollection(IIdentity<PersonCollection> collectionId)
        {
            var conv = idProv.GetConverter(typeof(PersonCollection), false);
            var collectionParams = conv.Convert(collectionId.Value).To<CollectionParameters>();
            var searchParams = conv.Convert(collectionId.Value).To<SimpleSearchParameters>();
            var skip = collectionParams?.Skip ?? 0;
            var limit = collectionParams?.Limit;
            var val = data.Values.Where(p => searchParams == null || p.FirstName.Contains(searchParams.Q) || p.LastName.Contains(searchParams.Q)).ToArray();

            return Task.FromResult(new PersonCollection(collectionId, val.Skip(skip).Take(limit ?? int.MaxValue), val.Length, collectionParams?.Limit, collectionParams?.Skip ?? 0));
        }
        public override Task<Person> Post(Person entity)
        {
            var id = entity.Id?.Value?.ToString();
            if (id == null)
            {
                string pk;
                do
                {
                    pk = Interlocked.Increment(ref counter).ToString();
                } while (data.ContainsKey(pk));
                entity = new Person(entity.FirstName, entity.LastName, entity.Age, FreeIdentity<Person>.Create(pk));
            }
            return Task.FromResult(data.AddOrUpdate(entity.Id.Value.ToString(), entity, (key, existing) => existing));
        }

        public override Task<Person> Put(Person entity)
            => Task.FromResult(data.AddOrUpdate(entity.Id.Value.ToString(), entity, (key, existing) => entity));


        public override CollectionRepository GetCollectionRepository() => new PersonCollectionRepository(this);

        protected override AbstractStructure GetStructure()
            => new Structure();

        public class Structure : AbstractStructure
        {
            public override string BasePath => "/person";
            public override Type WildcardType => typeof(PersonCollection.Parameters);
        }
    }

}
