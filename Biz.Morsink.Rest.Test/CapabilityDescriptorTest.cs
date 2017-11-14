using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.Rest.Test.Helpers;
using Biz.Morsink.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace Biz.Morsink.Rest.Test
{
    [TestClass]
    public class CapabilityDescriptorTest
    {


        [TestMethod]
        public void CapDesc_Get()
        {
            var repo = new TestGetRepo();
            var caps = repo.GetCapabilities().ToArray();
            Assert.AreEqual(1, caps.Length);
            var cap = caps[0];
            Assert.AreEqual(typeof(IRestGet<Person, Empty>), cap.InterfaceType);
            Assert.AreEqual(typeof(Person), cap.EntityType);
            Assert.AreEqual(typeof(Person), cap.ResultType);
            Assert.AreEqual(null, cap.BodyType);
            Assert.AreEqual("GET", cap.Name);
            Assert.IsNull(cap.BodyType);
        }
    }
}
