using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    using UPO = UnionRepresentation<SerializationTest.Person, SerializationTest.Organization>;
    [TestClass]
    public class SerializationTest
    {
        private ServiceProvider serviceProvider;
        private JsonRestSerializer serializer;

        [TestInitialize]
        public void Init()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ITypeDescriptorCreator, StandardTypeDescriptorCreator>();
            services.AddSingleton<IOptions<JsonHttpConverterOptions>, TestOptions>();
            services.AddSingleton<IOptions<RestAspNetCoreOptions>, TestRestOptions>();
            services.AddSingleton<IRestRequestScopeAccessor, TestRestRequestScopeAccessor>();
            services.AddSingleton<ICurrentHttpRestConverterAccessor, TestHttpRestConverterAccessor>();
            services.AddSingleton<IRestPrefixContainerAccessor, RestPrefixContainerAccessor>();

            services.AddTransient<ITypeRepresentation, IdentityRepresentation>();
            services.AddTransient<ITypeRepresentation, LinkRepresentation>();
            services.AddTransient<ITypeRepresentation, ExceptionRepresentation>();

            services.AddScoped<IAuthorizationProvider, AlwaysAllowAuthorizationProvider>();
            services.AddSingleton<IRestIdentityProvider, TestIdentityProvider>();

            services.AddSingleton<JsonRestSerializer>();
            services.AddJsonHttpConverter();
            serviceProvider = services.BuildServiceProvider();

            serializer = serviceProvider.GetRequiredService<JsonRestSerializer>();

        }
        [TestMethod]
        public void JsonSerialize_Normal()
        {
            var o = new A { B = 1, C = 2, D = "abc" };
            var json = serializer.WriteJson(o) as JObject;
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
            Assert.IsNotNull(json["D"]);

            var back = serializer.ReadJson<A>(json.CreateReader());
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_NormalImm()
        {
            var o = new ImmA(1, 2, "abc");
            var json = serializer.WriteJson(o) as JObject;
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
            Assert.IsNotNull(json["D"]);

            var back = serializer.ReadJson<ImmA>(json);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_NormalMixed()
        {
            var o = new MixedA(1, 2) { D = "abc" };
            var json = serializer.WriteJson(o) as JObject;
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
            Assert.IsNotNull(json["D"]);

            var back = serializer.ReadJson<MixedA>(json);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingStruct()
        {
            var o = new A { B = 1, D = "abc" };
            var json = serializer.WriteJson(o) as JObject;
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["D"]);

            var back = serializer.ReadJson<A>(json);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingStructImm()
        {
            var o = new ImmA(1, null, "abc");
            var json = serializer.WriteJson(o) as JObject;
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["D"]);

            var back = serializer.ReadJson<ImmA>(json);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingStructMixed()
        {
            var o = new MixedA(1, null) { D = "abc" };
            var json = serializer.WriteJson(o) as JObject;
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["D"]);

            var back = serializer.ReadJson<MixedA>(json);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingClass()
        {
            var o = new A { B = 1, C = 2 };
            var json = serializer.WriteJson(o) as JObject;
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);

            var back = serializer.ReadJson<A>(json);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingClassImm()
        {
            var o = new ImmA(1, 2, null);
            var json = serializer.WriteJson(o) as JObject;
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);

            var back = serializer.ReadJson<ImmA>(json);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingClassMixed()
        {
            var o = new MixedA(1, 2);
            var json = serializer.WriteJson(o) as JObject;
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);

            var back = serializer.ReadJson<MixedA>(json);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerializer_Embedding()
        {
            var nl = new Country { Code = "NL", Description = "The Netherlands" };
            var de = new Country { Code = "DE", Description = "Germany" };
            var me = new Person
            {
                Id = FreeIdentity<Person>.Create(1),
                FirstName = "Joost",
                LastName = "Joost",
                CountryOfResidence = FreeIdentity<Country>.Create("NL"),
                Nationality = FreeIdentity<Country>.Create("NL")
            };
            var wa = new Person
            {
                Id = FreeIdentity<Person>.Create(2),
                FirstName = "Willem-Alexander",
                LastName = "van Buren",
                CountryOfResidence = FreeIdentity<Country>.Create("NL"),
                Nationality = FreeIdentity<Country>.Create("DE") // Just a little dutch joke.
            };
            var rc = new Person
            {
                Id = FreeIdentity<Person>.Create(3),
                FirstName = "Rudi",
                LastName = "Carrell",
                CountryOfResidence = FreeIdentity<Country>.Create("DE"),
                Nationality = FreeIdentity<Country>.Create("NL")
            };
            nl.People.AddRange(new[] { me, wa });
            de.People.Add(rc);
            var json = serializer.WriteJson(Rest.Value(me).WithEmbeddings(new object[] { nl, de }));
            Assert.IsNotNull(json);
            Assert.IsNotNull(json["countryOfResidence"]);
            Assert.IsNotNull(json["nationality"]);
            Assert.AreEqual(4, json["countryOfResidence"].Children().Count());
            Assert.AreEqual(4, json["nationality"].Children().Count());
            Assert.IsNotNull(json["nationality"]["people"]);
            Assert.IsTrue(json["nationality"].Value<JArray>("people").OfType<JObject>().Any(o => o.Value<string>("href") == "/person/1" && o.Children().Count() == 1));
            Assert.IsTrue(json["nationality"].Value<JArray>("people").OfType<JObject>().All(o => o.Value<string>("href") == "/person/1" || o.Children().Count() > 1));
            Assert.IsTrue(JToken.DeepEquals(json["countryOfResidence"], json["nationality"]));
        }
        [TestMethod]
        public void JsonSerializer_UnionRep()
        {

            var scope = serviceProvider.GetRequiredService<IRestRequestScopeAccessor>().Scope;
            var idProv = serviceProvider.GetRequiredService<IRestIdentityProvider>();
            scope.SetScopeItem(SerializationContext.Create(idProv));
            scope.SetScopeItem(idProv.Prefixes);

            var joost = new Person
            {
                Id = FreeIdentity<Person>.Create(1),
                FirstName = "Joost",
                LastName = "Morsink",
                CountryOfResidence = FreeIdentity<Country>.Create("NL"),
                Nationality = FreeIdentity<Country>.Create("NL")
            };
            var morsinkSoftware = new Organization
            {
                Id = FreeIdentity<Organization>.Create(1),
                LegalName = "Morsink Software",
                ChamberOfCommerceNo = "12345678",
                CountryOfEstablishment = FreeIdentity<Country>.Create("NL")
            };
            var json = serializer.WriteJson(new UPO.Option1(joost)) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(5, json.Properties().Count());
            Assert.AreEqual("Joost", json.Value<string>("firstName"));
            Assert.AreEqual("Morsink", json.Value<string>("lastName"));

            var up = serializer.ReadJson<UPO>(json);
            if (up is UPO.Option1 p)
            {
                Assert.AreEqual("Joost", p.Item.FirstName);
                Assert.AreEqual("Morsink", p.Item.LastName);
            }
            else
                Assert.Fail("Wrong option/type.");

            json = serializer.WriteJson(new UPO.Option2(morsinkSoftware)) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(4, json.Properties().Count());
            Assert.AreEqual("Morsink Software", json.Value<string>("legalName"));
            Assert.AreEqual("12345678", json.Value<string>("chamberOfCommerceNo"));

            var uo = serializer.ReadJson<UPO>(json);
            if (uo is UPO.Option2 o)
            {
                Assert.AreEqual("Morsink Software", o.Item.LegalName);
                Assert.AreEqual("12345678", o.Item.ChamberOfCommerceNo);
            }
            else
                Assert.Fail("Wrong option/type.");

        }
        [TestMethod]
        public void JsonSerializer_Primitives()
        {
            var json = serializer.WriteJson(42);
            Assert.IsNotNull(json);
            if (json is JValue val)
                Assert.AreEqual(42, val.Value);
            else
                Assert.Fail();

            json = serializer.WriteJson("abc");
            Assert.IsNotNull(json);
            if (json is JValue val2)
                Assert.AreEqual("abc", val2.Value);
            else
                Assert.Fail();

            json = serializer.WriteJson(new DateTime(2018, 10, 16, 11, 06, 0, DateTimeKind.Utc));
            Assert.IsNotNull(json);
            if (json is JValue val3)
                Assert.AreEqual("2018-10-16T11:06:00.000Z", val3.Value);
            else
                Assert.Fail();



        }
        // Would like to have this work, but does not work in original Json.Net either...
        //[TestMethod]
        //public void JsonSerialize_DefaultCtorParam()
        //{
        //    var o = new DefCtorParamA(1, 2);

        //    var json = JObject.Parse("{B:1, C:2}");
        //    var back = json.ToObject<DefCtorParamA>(serializer);
        //    Assert.IsTrue(back.Equals(o));
        //    Assert.AreEqual("xyz", back.D);
        //}
        public class A : IEquatable<A>
        {
            public int B { get; set; }
            public int? C { get; set; }
            public string D { get; set; }
            public override int GetHashCode()
                => B;
            public override bool Equals(object obj)
                => obj is A a ? Equals(a) : false;
            public bool Equals(A other)
                => B == other.B && C == other.C && D == other.D;
            public static bool operator ==(A a, A b)
                => a.Equals(b);
            public static bool operator !=(A a, A b)
                => !a.Equals(b);
        }
        public class ImmA : IEquatable<ImmA>
        {
            public ImmA(int b, int? c, string d)
            {
                B = b;
                C = c;
                D = d;
            }
            public int B { get; }
            public int? C { get; }
            public string D { get; }
            public override int GetHashCode()
                => B;
            public override bool Equals(object obj)
                => obj is ImmA a ? Equals(a) : false;
            public bool Equals(ImmA other)
                => B == other.B && C == other.C && D == other.D;
            public static bool operator ==(ImmA a, ImmA b)
                => a.Equals(b);
            public static bool operator !=(ImmA a, ImmA b)
                => !a.Equals(b);
        }
        public class MixedA : IEquatable<MixedA>
        {
            public MixedA(int b, int? c)
            {
                B = b;
                C = c;
            }
            public int B { get; set; }
            public int? C { get; set; }
            public string D { get; set; }
            public override int GetHashCode()
                => B;
            public override bool Equals(object obj)
                => obj is MixedA a ? Equals(a) : false;
            public bool Equals(MixedA other)
                => B == other.B && C == other.C && D == other.D;
            public static bool operator ==(MixedA a, MixedA b)
                => a.Equals(b);
            public static bool operator !=(MixedA a, MixedA b)
                => !a.Equals(b);
        }
        public class Person : IHasIdentity<Person>
        {
            IIdentity IHasIdentity.Id => Id;
            public IIdentity<Person> Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public IIdentity<Country> Nationality { get; set; }
            public IIdentity<Country> CountryOfResidence { get; set; }
        }
        public class Organization : IHasIdentity<Organization>
        {
            IIdentity IHasIdentity.Id => Id;
            public IIdentity<Organization> Id { get; set; }
            public string LegalName { get; set; }
            public string ChamberOfCommerceNo { get; set; }
            public IIdentity<Country> CountryOfEstablishment { get; set; }
        }
        public class Country : IHasIdentity<Country>
        {
            IIdentity IHasIdentity.Id => Id;
            public IIdentity<Country> Id { get; set; }
            public string Code { get => Id?.Value?.ToString(); set => Id = FreeIdentity<Country>.Create(value); }
            public string Description { get; set; }
            public List<Person> People { get; set; } = new List<Person>();
        }
    }
}
