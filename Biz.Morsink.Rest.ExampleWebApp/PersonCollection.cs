using Biz.Morsink.Identity;
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

        public PersonCollection(IIdentity<PersonCollection> id, IEnumerable<Person> persons, int count, int? limit = null, int skip = 0) : base(id,persons, count, limit, skip)
        {
        }
        public new IIdentity<PersonCollection> Id => (IIdentity<PersonCollection>)base.Id;
    }
}
