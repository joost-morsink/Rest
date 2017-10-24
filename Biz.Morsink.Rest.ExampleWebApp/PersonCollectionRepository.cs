using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class PersonCollectionRepository : RestRepository<PersonCollection>, IRestGet<PersonCollection, NoParameters>
    {
        private readonly IRestResourceCollection<PersonCollection, Person> resources;

        public PersonCollectionRepository(IRestResourceCollection<PersonCollection, Person> resources)
        {
            this.resources = resources;
        }

        public ValueTask<RestResponse<PersonCollection>> Get(IIdentity<PersonCollection> id, NoParameters parameters)
            => Rest.Value(resources.GetCollection(id)).ToResponseAsync();

    }
}
