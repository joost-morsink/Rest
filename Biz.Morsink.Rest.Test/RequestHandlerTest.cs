using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.Test
{
    [TestClass]
    public class RequestHandlerTest
    {
        [TestMethod]
        public async Task RequestHandler_HappyGet()
        {
            var testrepo = new TestGetRepo();
            var handler = new RestRequestHandler(new[] { testrepo });
            var req = new RestRequest("GET", FreeIdentity<Person>.Create(1), Enumerable.Empty<(string, string)>(), ty => new object());
            var response = await handler.HandleRequest(req) as RestResponse<Person>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);

            var val = response.Value.AsSuccess().RestValue.Value;
            Assert.AreEqual("Joost", val.FirstName);
            Assert.AreEqual("Morsink", val.LastName);
            Assert.AreEqual(37, val.Age);
        }
    }
}
