using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Test
{
    [TestClass]
    public class RestIdentityProviderTest
    {
        private readonly RestIdentityProvider provider;

        public RestIdentityProviderTest()
        {
            provider = new TestRestIdentityProvider(null,new TypeDescriptorCreator());
        }
        [TestMethod]
        public void RestIdProv_HappySingle()
        {
            var p = provider.Parse<Person>("/api/person/1");
            Assert.IsNotNull(p);
            Assert.AreEqual("1", p.Value.ToString());
            var g = provider.ToGeneralIdentity(p);
            Assert.IsNotNull(g);
            Assert.AreEqual("/api/person/1", g.Value.ToString());
        }
        [TestMethod]
        public void RestIdProv_HappySearch()
        {
            var p = provider.Parse<PersonCollection>("/api/person?search=Morsink");
            Assert.IsNotNull(p);
            var d = p.Value as Dictionary<string, string>;
            Assert.AreEqual(1, d.Count);
            Assert.IsTrue(d.TryGetValue("search", out var srch));
            Assert.AreEqual("Morsink", srch);
            var g = provider.ToGeneralIdentity(p);
            Assert.IsNotNull(g);
            Assert.AreEqual("/api/person?search=Morsink", g.Value.ToString());
        }
        [TestMethod]
        public void RestIdProv_HappyDetailSearch()
        {
            var p = provider.Parse<FriendCollection>("/api/person/1/friends");
            Assert.IsNotNull(p);
            var d = p.ComponentValue as Dictionary<string, string>;
            Assert.AreEqual(0, d.Count);
            Assert.AreEqual("1", p.For<Person>().ComponentValue.ToString());
        }

    }
    public class TestRestIdentityProvider : RestIdentityProvider
    {
        public TestRestIdentityProvider(IEnumerable<IRestRepository> repositories, TypeDescriptorCreator tdc) : base(repositories ?? Enumerable.Empty<IRestRepository>(), tdc)
        {
            BuildEntry(typeof(PersonCollection)).WithPath("/api/person?*").Add();
            BuildEntry(typeof(Person)).WithPath("/api/person/*").Add();
            BuildEntry(typeof(Person), typeof(FriendCollection)).WithPath("/api/person/*/friends?*").Add();
        }

    }
    public class PersonCollection
    {

    }
    public class Person
    {

    }
    public class FriendCollection
    {

    }
}
