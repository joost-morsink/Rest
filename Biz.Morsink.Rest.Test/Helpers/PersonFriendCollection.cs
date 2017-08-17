using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Test.Helpers
{
    public class PersonFriendCollection
    {
        public IIdentity<Person> PersonId { get; set; }
        public IIdentity<Person>[] FriendIds { get; set; }
    }
}
