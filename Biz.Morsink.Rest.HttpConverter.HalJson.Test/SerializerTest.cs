using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.DataConvert;
using System;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Collections.Generic;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Biz.Morsink.Identity.PathProvider;

namespace Biz.Morsink.Rest.HttpConverter.HalJson.Test
{
    [TestClass]
    public class SerializerTest
    {
        public class HelperA : IHasIdentity<HelperA>
        {
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }

            public IIdentity<HelperA> Id => FreeIdentity<HelperA>.Create(A);

            IIdentity IHasIdentity.Id => Id;
        }
        public class HelperB : IHasIdentity<HelperB>
        {
            public HelperB(string a, string b, string c)
            {
                A = a;
                B = b;
                C = c;
            }
            public string A { get; }
            public string B { get; }
            public string C { get; }

            public IIdentity<HelperB> Id => FreeIdentity<HelperB>.Create(B);

            IIdentity IHasIdentity.Id => Id;
        }
        public class HelperC : IHasIdentity<HelperC>
        {
            public string A { get; set; }
            public HelperA[] As { get; set; }
            public List<HelperB> Bs { get; set; }
            public IReadOnlyCollection<HelperA> MoreAs { get; set; }

            public IIdentity<HelperC> Id => FreeIdentity<HelperC>.Create(A);

            IIdentity IHasIdentity.Id => Id;
        }
        public class Container
        {
            public EmailAddress Email { get; set; }
        }
        public struct EmailAddress
        {
            public EmailAddress(string address)
            {
                Address = address;
            }

            public string Address { get; }
        }
        public class TestRestIdentityProvider : RestIdentityProvider
        {
            public TestRestIdentityProvider() : base()
            {
                BuildEntry(typeof(HelperA)).WithPath("/a/*").Add();
                BuildEntry(typeof(HelperB)).WithPath("/b/*").Add();
                BuildEntry(typeof(HelperC)).WithPath("/c/*").Add();
            }

        }
        public class TestRestPrefixContainerAccessor : IRestPrefixContainerAccessor
        {
            public TestRestPrefixContainerAccessor(IRestIdentityProvider identityProvider)
            {
                RestPrefixContainer = identityProvider.Prefixes;
            }
            public RestPrefixContainer RestPrefixContainer { get; }
        }
        public class TestRestOptions : IOptions<RestAspNetCoreOptions>
        {
            public RestAspNetCoreOptions Value
                => new RestAspNetCoreOptions
                {
                    UseCuries = false
                };
        }
        public class TestHalOptions : IOptions<HalJsonConverterOptions>
        {
            public HalJsonConverterOptions Value
                => new HalJsonConverterOptions
                {

                };
        }
        private ServiceProvider serviceProvider;
        private HalJsonRestSerializer serializer;
        private Func<SerializationContext> context;
        [TestInitialize]
        public void Init()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ITypeDescriptorCreator, StandardTypeDescriptorCreator>();
            services.AddSingleton<IOptions<HalJsonConverterOptions>, TestHalOptions>();
            services.AddSingleton<IOptions<RestAspNetCoreOptions>, TestRestOptions>();
            services.AddSingleton<IRestRequestScopeAccessor, TestRestRequestScopeAccessor>();
            services.AddSingleton<ICurrentHttpRestConverterAccessor, TestHttpRestConverterAccessor>();
            services.AddSingleton<IRestPrefixContainerAccessor, RestPrefixContainerAccessor>();

            services.AddTransient<ITypeRepresentation, IdentityRepresentation>();
            services.AddTransient<ITypeRepresentation, LinkRepresentation>();
            services.AddTransient<ITypeRepresentation, ExceptionRepresentation>();

            services.AddScoped<IAuthorizationProvider, AlwaysAllowAuthorizationProvider>();
            services.AddSingleton<IRestIdentityProvider, TestRestIdentityProvider>();

            services.AddSingleton<HalJsonRestSerializer>();
            services.AddHalJsonHttpConverter();
            serviceProvider = services.BuildServiceProvider();

            serializer = serviceProvider.GetRequiredService<HalJsonRestSerializer>();
            context = () => SerializationContext.Create(serviceProvider.GetRequiredService<IRestIdentityProvider>());

            var converter = DataConverter.Default;
            //identityProvider = new TestRestIdentityProvider();
            //var typereps = new ITypeRepresentation[] {
            //    new IdentityRepresentation(identityProvider, new TestRestPrefixContainerAccessor(identityProvider), new TestRestOptions(), null)
            //};
            //var tdc = new StandardTypeDescriptorCreator(typereps);
            //serializer = new HalSerializer(tdc, converter, identityProvider, typereps);
            //context = new Func<HalContext>(() => HalContext.Create(identityProvider));
        }
        [TestMethod]
        public void HalSerializer_Primitives()
        {
            Assert.AreEqual("123", serializer.WriteJson(123).ToString());
            Assert.AreEqual("xyz", ((JValue)serializer.WriteJson("xyz")).Value);
            Assert.AreEqual("2018-07-03T07:53:00.000Z", ((JValue)serializer.WriteJson(new DateTime(2018, 7, 3, 7, 53, 0, DateTimeKind.Utc))).ToString());
            Assert.AreEqual("12.340", ((JValue)serializer.WriteJson(12.340m)).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("12.34", ((JValue)serializer.WriteJson(12.340)).ToString(CultureInfo.InvariantCulture));
        }
        [TestMethod]
        public void HalSerializer_PrimitivesBack()
        {
            Assert.AreEqual(123, serializer.ReadJson<int>(JToken.Parse("123")));
            Assert.AreEqual("xyz", serializer.ReadJson<string>(JToken.Parse("\"xyz\"")));
            Assert.AreEqual(new DateTime(2018, 7, 3, 7, 53, 0, DateTimeKind.Utc), serializer.ReadJson<DateTime>(JToken.Parse("\"2018-07-03T07:53:00.000Z\"")));
            Assert.AreEqual(12.34m, serializer.ReadJson<decimal>(JToken.Parse("12.340")));
            Assert.AreEqual(12.34, serializer.ReadJson<double>(JToken.Parse("12.34")), 0.000_001);
        }
        [TestMethod]
        public void HalSerializer_MutableRecords()
        {
            var a = new HelperA { A = "a", B = "b", C = "c" };
            var json = serializer.WriteJson(a) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(4, json.Properties().Count());
            Assert.IsNotNull(json["a"]);
            Assert.IsNotNull(json["b"]);
            Assert.IsNotNull(json["c"]);
            var dea = serializer.ReadJson<HelperA>(json);
            Assert.AreEqual("a", dea.A);
            Assert.AreEqual("b", dea.B);
            Assert.AreEqual("c", dea.C);

        }
        [TestMethod]
        public void HalSerializer_ImmutableRecords()
        {
            var b = new HelperB("a", "b", "c");
            var json = serializer.WriteJson(b) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(4, json.Properties().Count());
            Assert.IsNotNull(json["a"]);
            Assert.IsNotNull(json["b"]);
            Assert.IsNotNull(json["c"]);
            var deb = serializer.ReadJson<HelperB>(json);
            Assert.AreEqual("a", deb.A);
            Assert.AreEqual("b", deb.B);
            Assert.AreEqual("c", deb.C);
        }
        [TestMethod]
        public void HalSerializer_Collections()
        {
            var c = new HelperC
            {
                A = "123",
                As = new[] { new HelperA { A = "a", B = "b", C = "c" }, new HelperA { A = "d", B = "e", C = "f" } },
                Bs = new List<HelperB> { new HelperB("A", "B", "C"), new HelperB("X", "Y", "Z") },
                MoreAs = new[] { new HelperA { A = "!", B = "@", C = "#" } }
            };
            var json = serializer.WriteJson(c) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(5, json.Properties().Count());
            Assert.IsNotNull(json["a"]);
            Assert.IsNotNull(json["as"]);
            Assert.AreEqual(2, json.Value<JArray>("as").Count);
            Assert.IsNotNull(json["bs"]);
            Assert.AreEqual(2, json.Value<JArray>("bs").Count);
            Assert.IsNotNull(json["moreAs"]);
            Assert.AreEqual(1, json.Value<JArray>("moreAs").Count);

            var dec = serializer.ReadJson<HelperC>(json);
            Assert.AreEqual("123", dec.A);
            Assert.AreEqual(2, dec.As.Length);
            Assert.AreEqual("e", dec.As[1].B);
            Assert.AreEqual(2, dec.Bs.Count);
            Assert.AreEqual("Z", dec.Bs[1].C);
            Assert.AreEqual(1, dec.MoreAs.Count);
            Assert.AreEqual("!", dec.MoreAs.First().A);
        }
        [TestMethod]
        public void HalSerializer_Dictionaries()
        {
            var x = new Dictionary<string, object>
            {
                ["A"] = 1,
                ["B"] = "abc",
                ["C"] = DateTime.UtcNow
            };
            var json = serializer.WriteJson(x) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["a"]);
            Assert.IsNotNull(json["b"]);
            Assert.IsNotNull(json["c"]);
        }
        public void XmlSerializer_SortedDictionaries()
        {
            var sortedDictioary = new SortedDictionary<string, string>()
            {
                ["A"] = "1",
                ["B"] = "abc",
                ["C"] = "42x"
            };
            CheckDictionary<SortedDictionary<string, string>, string>(sortedDictioary);
        }
        [TestMethod]
        public void HalSerializer_ImmutableDictionaries()
        {

            var immutableDictionary = ImmutableDictionary<string, string>.Empty
                .Add("A", "1")
                .Add("B", "2")
                .Add("C", "3")
                .Add("D", "4");
            CheckDictionary<ImmutableDictionary<string, string>, string>(immutableDictionary);
        }
        private void CheckDictionary<T, V>(T x)
            where T : IReadOnlyDictionary<string, V>
        {
            var json = serializer.WriteJson(x) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(x.Count, json.Children().Count());
            Assert.IsTrue(x.Keys.All(key => json[key.ToLower()] != null));

            var dict = serializer.ReadJson<T>(json);
            Assert.AreEqual(x.Count, dict.Count);
            Assert.AreEqual(x.Count, dict.Keys.Join(x.Keys, a => a, b => b, (a, b) => a, CaseInsensitiveEqualityComparer.Instance).Count());
        }
        [TestMethod]
        public void HalSerializer_SemStr()
        {
            var x = new Container { Email = new EmailAddress("info@test.nl") };
            var json = serializer.WriteJson(x) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(1, json.Properties().Count());
            Assert.AreEqual(nameof(Container.Email), json.Properties().First().Name, true);
            Assert.AreEqual("info@test.nl", json.Properties().First().Value);

        }
        [TestMethod]
        public void HalSerializer_RestValue()
        {
            var a1 = new HelperA { A = "a", B = "b", C = "c" };
            var a2 = new HelperA { A = "d", B = "e", C = "f" };
            var b1 = new HelperB("A", "B", "C");
            var b2 = new HelperB("X", "Y", "Z");
            var c = new HelperC
            {
                A = "123",
                As = new[] { a1, a2 },
                Bs = new List<HelperB> { b1, b2 },
                MoreAs = new[] { new HelperA { A = "!", B = "@", C = "#" } }
            };
            var rv = RestValue<HelperC>.Build()
                .WithValue(c)
                .WithEmbeddings(new [] { new Embedding("a", a1), new Embedding("a", a2), new Embedding("b",b1), new Embedding("b", b2) })
                .WithLink(Link.Create("self", c.Id))
                .Build();
            var json = serializer.WriteJson(rv) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(7, json.Properties().Count());

            Assert.IsNotNull(json["a"]);
            Assert.IsNotNull(json["as"]);
            Assert.AreEqual(2, json.Value<JArray>("as").Count);
            Assert.IsTrue(json.Value<JArray>("as").All(t => t.Children().Count() == 1));

            Assert.IsNotNull(json["bs"]);
            Assert.AreEqual(2, json.Value<JArray>("bs").Count);
            Assert.IsTrue(json.Value<JArray>("bs").All(t => t.Children().Count() == 1));

            Assert.IsNotNull(json["moreAs"]);
            Assert.AreEqual(1, json.Value<JArray>("moreAs").Count);

            Assert.IsNotNull(json["_embedded"]);
            var embedded = json.Value<JObject>("_embedded");
            Assert.AreEqual(2, embedded.Properties().Count());
            Assert.IsNotNull(embedded["a"]);
            Assert.IsNotNull(embedded["b"]);
            Assert.AreEqual(2, embedded.Value<JArray>("a").Count);
            Assert.AreEqual(2, embedded.Value<JArray>("b").Count);

            Assert.IsNotNull(json["_links"]);
            Assert.AreEqual(1, json["_links"].Children().Count());
            Assert.IsNotNull(json["_links"]["self"]);

            //var dec = serializer.Deserialize<HelperC>(context(), json);
            //Assert.AreEqual("123", dec.A);
            //Assert.AreEqual(2, dec.As.Length);
            //Assert.AreEqual("e", dec.As[1].B);
            //Assert.AreEqual(2, dec.Bs.Count);
            //Assert.AreEqual("Z", dec.Bs[1].C);
            //Assert.AreEqual(1, dec.MoreAs.Count);
            //Assert.AreEqual("!", dec.MoreAs.First().A);
        }
    }
}
