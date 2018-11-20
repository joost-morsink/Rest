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
            cb.RegisterType<AutofacServiceProvider>().AsImplementedInterfaces();
            cb.RegisterType<CoreRestRequestHandler>().AsImplementedInterfaces().SingleInstance();
            cb.RegisterInstance(ServiceProviderAccessor.Instance).AsImplementedInterfaces();
            return cb;
        }
        private RestRequest CreateRequest(IContainer container, string cap, IIdentity target, IEnumerable<(string, string)> parameters = null)
        {
            var sp = container.Resolve<IServiceProvider>();
            ServiceProviderAccessor.Instance.ServiceProvider = sp;
            return RestRequest.Create(cap, target, parameters?.Select(x => new KeyValuePair<string, string>(x.Item1, x.Item2)), null, null);
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
                var req = CreateRequest(cont, "GET", FreeIdentity<Person>.Create(1));
                var response = await handler.HandleRequest(req) as RestResponse<Person>;
                Assert.IsNotNull(response);
                Assert.IsTrue(response.IsSuccess);

                var val = response.Result.AsSuccess().RestValue.Value;
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
                var req = CreateRequest(cont, "GET", FreeIdentity<Person2>.Create(1), new[] { ("ageFactor", "1.5") });
                var response = await handler.HandleRequest(req) as RestResponse<Person2>;
                Assert.IsNotNull(response);
                Assert.IsTrue(response.IsSuccess);
                var val = response.Result.AsSuccess().RestValue.Value;
                Assert.AreEqual("Joost", val.FirstName);
                Assert.AreEqual("Morsink", val.LastName);
                Assert.AreEqual(55, val.Age);

                req = CreateRequest(cont, "GET", FreeIdentity<Person2>.Create(1));
                response = await handler.HandleRequest(req) as RestResponse<Person2>;
                Assert.IsNotNull(response);
                Assert.IsTrue(response.IsSuccess);
                val = response.Result.AsSuccess().RestValue.Value;
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
                var req = CreateRequest(cont, "GET", id);
                var response = await handler.HandleRequest(req) as RestResponse<Person>;
                Assert.IsNotNull(response);
                Assert.IsTrue(response.IsSuccess);
                Assert.AreEqual(1, response.Result.AsSuccess().Links.Count);
                Assert.AreEqual(typeof(PersonFriendCollection), response.Result.AsSuccess().Links.First().Target.ForType);

                var req2 = CreateRequest(cont, "GET", response.Result.AsSuccess().Links.First().Target);
                var response2 = await handler.HandleRequest(req2) as RestResponse<PersonFriendCollection>;
                Assert.IsNotNull(response2);
                Assert.IsTrue(response2.IsSuccess);
                Assert.AreEqual(id, response2.Result.AsSuccess().Value.PersonId);
                Assert.AreEqual(1, response2.Result.AsSuccess().Value.FriendIds.Length);
                Assert.AreEqual(id, response2.Result.AsSuccess().Value.FriendIds[0]);

            }
        }
    }
}
