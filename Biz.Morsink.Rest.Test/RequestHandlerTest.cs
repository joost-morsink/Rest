using Autofac;
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
        private ContainerBuilder CreateContainerBuilder()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<AutofacServiceLocator>().AsImplementedInterfaces();
            cb.RegisterType<RestRequestHandler>().AsImplementedInterfaces().SingleInstance();
            return cb;
        }
        
        [TestMethod]
        public async Task RequestHandler_HappyGet()
        {
            var cb = CreateContainerBuilder();
            cb.RegisterType<TestGetRepo>().AsImplementedInterfaces().SingleInstance();
            var cont = cb.Build();

            using (var lts = cont.BeginLifetimeScope())
            {
                var handler = lts.Resolve<IRestRequestHandler>(); 
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
        [TestMethod]
        public async Task RequestHandler_HappyGetParameter()
        {
            var cb = CreateContainerBuilder();
            cb.RegisterType<TestGetRepo2>().AsImplementedInterfaces().SingleInstance();
            var cont = cb.Build();
            using (var lts = cont.BeginLifetimeScope())
            {
                var handler = lts.Resolve<IRestRequestHandler>();
                var req = new RestRequest("GET", FreeIdentity<Person2>.Create(1), new[] { ("ageFactor", "1.5") }, ty => new object());
                var response = await handler.HandleRequest(req) as RestResponse<Person2>;
                Assert.IsNotNull(response);
                Assert.IsTrue(response.IsSuccess);
                var val = response.Value.AsSuccess().RestValue.Value;
                Assert.AreEqual("Joost", val.FirstName);
                Assert.AreEqual("Morsink", val.LastName);
                Assert.AreEqual(55, val.Age);

                req = new RestRequest("GET", FreeIdentity<Person2>.Create(1), Enumerable.Empty<(string, string)>(), ty => new object());
                response = await handler.HandleRequest(req) as RestResponse<Person2>;
                Assert.IsNotNull(response);
                Assert.IsTrue(response.IsSuccess);
                val = response.Value.AsSuccess().RestValue.Value;
                Assert.AreEqual("Joost", val.FirstName);
                Assert.AreEqual("Morsink", val.LastName);
                Assert.AreEqual(37, val.Age);
            }
        }
        [TestMethod]
        public async Task RequestHandler_HappyGetLink()
        {
            var cb = CreateContainerBuilder();
            cb.RegisterType<TestGetRepo>().AsImplementedInterfaces();
            cb.RegisterType<TestGetFriendCollectionRepo>().AsImplementedInterfaces();
            cb.RegisterType<PersonFriendCollectionLinkProvider>().AsImplementedInterfaces();
            var cont = cb.Build();

            using (var lts = cont.BeginLifetimeScope())
            {
                var handler = lts.Resolve<IRestRequestHandler>();
                var id = FreeIdentity<Person>.Create(1);
                var req = new RestRequest("GET", id, Enumerable.Empty<(string, string)>(), ty => new object());
                var response = await handler.HandleRequest(req) as RestResponse<Person>;
                Assert.IsNotNull(response);
                Assert.IsTrue(response.IsSuccess);
                Assert.AreEqual(1, response.Value.AsSuccess().Links.Count);
                Assert.AreEqual(typeof(PersonFriendCollection), response.Value.AsSuccess().Links.First().Target.ForType);

                var req2 = new RestRequest("GET", response.Value.AsSuccess().Links.First().Target, Enumerable.Empty<(string, string)>(), ty => new object());
                var response2 = await handler.HandleRequest(req2) as RestResponse<PersonFriendCollection>;
                Assert.IsNotNull(response2);
                Assert.IsTrue(response2.IsSuccess);
                Assert.AreEqual(id, response2.Value.AsSuccess().Value.PersonId);
                Assert.AreEqual(1, response2.Value.AsSuccess().Value.FriendIds.Length);
                Assert.AreEqual(id, response2.Value.AsSuccess().Value.FriendIds[0]);

            }
        }
    }
}
