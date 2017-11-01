using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.DataConvert;
namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class PersonSource : IRestResourceCollection<PersonCollection, Person>
    {
        private int counter = 1;

        private ConcurrentDictionary<string, Person> data = new ConcurrentDictionary<string, Person>
        {
            ["1"] = new Person("Joost", "Morsink", 38, FreeIdentity<Person>.Create(1))
        };
        private readonly IRestIdentityProvider idProv;

        public PersonSource(IRestIdentityProvider idProv)
        {
            this.idProv = idProv;
        }

        public Task<bool> Delete(IIdentity<Person> entityId)
            => Task.FromResult(data.TryRemove(entityId.Value?.ToString(), out var _));


        public Task<Person> Get(IIdentity<Person> entityId)
            => Task.FromResult(data.TryGetValue(entityId.Value?.ToString(), out var p) ? p : null);

        public Task<PersonCollection> GetCollection(IIdentity<PersonCollection> collectionId)
        {
            var conv = idProv.GetConverter(typeof(PersonCollection), false);
            var collectionParams = conv.Convert(collectionId.Value).To<CollectionParameters>();
            var searchParams = conv.Convert(collectionId.Value).To<SimpleSearchParameters>();
            var skip = collectionParams?.Skip ?? 0;
            var limit = collectionParams?.Limit;
            var val = data.Values.Where(p => searchParams == null || p.FirstName.Contains(searchParams.Q) || p.LastName.Contains(searchParams.Q)).ToArray();

            return Task.FromResult(new PersonCollection(collectionId, val.Skip(skip).Take(limit ?? int.MaxValue), val.Length, collectionParams?.Limit, collectionParams?.Skip ?? 0));
        }
        public Task<Person> Post(Person entity)
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

        public Task<Person> Put(Person entity)
            => Task.FromResult(data.AddOrUpdate(entity.Id.Value.ToString(), entity, (key, existing) => entity));

    }
}
