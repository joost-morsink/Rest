using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A PersonCollection.
    /// </summary>
    public class PersonCollection : RestCollection<Person>
    {
        public class Parameters
        {
            public int Skip { get; set; }
            public int? Limit { get; set; }
            public string Q { get; set; }
        }
        public PersonCollection(IIdentity<PersonCollection> id, IEnumerable<Person> items, int count, int? limit = null, int skip = 0)
            : base(id, items, count, limit, skip)
        {
        }
        public new IIdentity<PersonCollection> Id => (IIdentity<PersonCollection>)base.Id;
    }
    /// <summary>
    /// A PersonCollection.
    /// </summary>
    public class PersonV2Collection : RestCollection<PersonV2>
    {
        public class Parameters
        {
            public int Skip { get; set; }
            public int? Limit { get; set; }
            public string Q { get; set; }
        }
        public PersonV2Collection(IIdentity<PersonV2Collection> id, IEnumerable<PersonV2> items, int count, int? limit = null, int skip = 0) 
            : base(id, items, count, limit, skip)
        {
        }
        public new IIdentity<PersonV2Collection> Id => (IIdentity<PersonV2Collection>)base.Id;
        public PersonCollection ToV1()
            => new PersonCollection(FreeIdentity<PersonCollection>.Create(Id.Value), Items.Select(x => x.ToV1()), Count, Limit, Skip);
        
    }

}
