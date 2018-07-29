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
    public class PersonSource : IRestResourceCollection<PersonV2Collection, PersonV2>
    {
        private int counter = 1;

        private ConcurrentDictionary<string, PersonV2> data = new ConcurrentDictionary<string, PersonV2>
        {
            ["1"] = new PersonV2("Joost", "Morsink", DateTime.Now.Date.AddYears(- 38), FreeIdentity<PersonV2>.Create(1))
        };
        private readonly IRestIdentityProvider idProv;

        public PersonSource(IRestIdentityProvider idProv)
        {
            this.idProv = idProv;
        }

        public Task<bool> Delete(IIdentity<PersonV2> entityId)
            => Task.FromResult(data.TryRemove(entityId.Value?.ToString(), out var _));


        public Task<PersonV2> Get(IIdentity<PersonV2> entityId)
            => Task.FromResult(data.TryGetValue(entityId.Value?.ToString(), out var p) ? p : null);

        public Task<PersonV2Collection> GetCollection(IIdentity<PersonV2Collection> collectionId)
        {
            var conv = idProv.GetConverter(typeof(PersonCollection), false);
            var collectionParams = conv.Convert(collectionId.Value).To<CollectionParameters>();
            var searchParams = conv.Convert(collectionId.Value).To<SimpleSearchParameters>();
            var skip = collectionParams?.Skip ?? 0;
            var limit = collectionParams?.Limit;
            var val = data.Values.Where(p => searchParams == null || p.FirstName.Contains(searchParams.Q) || p.LastName.Contains(searchParams.Q)).ToArray();

            return Task.FromResult(new PersonV2Collection(collectionId, val.Skip(skip).Take(limit ?? int.MaxValue), val.Length, collectionParams?.Limit, collectionParams?.Skip ?? 0));
        }
        public Task<PersonV2> Post(PersonV2 resource)
        {
            var id = resource.Id?.Value?.ToString();
            if (id == null)
            {
                string pk;
                do
                {
                    pk = Interlocked.Increment(ref counter).ToString();
                } while (data.ContainsKey(pk));
                resource = new PersonV2(resource.FirstName, resource.LastName, resource.Birthday, FreeIdentity<PersonV2>.Create(pk));
            }
            return Task.FromResult(data.AddOrUpdate(resource.Id.Value.ToString(), resource, (key, existing) => existing));
        }

        public Task<PersonV2> Put(PersonV2 resource)
            => Task.FromResult(data.AddOrUpdate(resource.Id.Value.ToString(), resource, (key, existing) => resource));

    }
}
