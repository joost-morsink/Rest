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
    [TestClass]
    public class SerializationTest
    {
        private TestRestRequestScopeAccessor restRequestScopeAccessor;
        private TestOptions options;
        private TestIdentityProvider identityProvider;
        private RestJsonContractResolver resolver;
        private ServiceProvider serviceProvider;
        private JsonSerializer serializer;

        [TestInitialize]
        public void Init()
        {
            var services = new ServiceCollection();

            services.AddSingleton<TypeDescriptorCreator>();
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

            //services.AddSingleton<IContractResolver, RestJsonContractResolver>();
            services.AddJsonHttpConverter();
            serviceProvider = services.BuildServiceProvider();

            serializer = JsonSerializer.Create(serviceProvider.GetRequiredService<IOptions<JsonHttpConverterOptions>>().Value.SerializerSettings);

            serializer.ContractResolver = serviceProvider.GetRequiredService<IContractResolver>();

        }
        [TestMethod]
        public void JsonSerialize_Normal()
        {
            var o = new A { B = 1, C = 2, D = "abc" };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<A>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_NormalImm()
        {
            var o = new ImmA(1, 2, "abc");
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<ImmA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_NormalMixed()
        {
            var o = new MixedA(1, 2) { D = "abc" };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<MixedA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingStruct()
        {
            var o = new A { B = 1, D = "abc" };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<A>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingStructImm()
        {
            var o = new ImmA(1, null, "abc");
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<ImmA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingStructMixed()
        {
            var o = new MixedA(1, null) { D = "abc" };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<MixedA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingClass()
        {
            var o = new A { B = 1, C = 2 };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);

            var back = json.ToObject<A>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingClassImm()
        {
            var o = new ImmA(1, 2, null);
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);

            var back = json.ToObject<ImmA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingClassMixed()
        {
            var o = new MixedA(1, 2);
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);

            var back = json.ToObject<MixedA>(serializer);
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
            var json = JObject.FromObject(Rest.Value(me).WithEmbeddings(new object[] { nl, de }), serializer);
            Assert.IsNotNull(json);
            Assert.IsNotNull(json["CountryOfResidence"]);
            Assert.IsNotNull(json["Nationality"]);
            Assert.AreEqual(4, json["CountryOfResidence"].Children().Count());
            Assert.AreEqual(4, json["Nationality"].Children().Count());
            Assert.IsNotNull(json["Nationality"]["People"]);
            Assert.IsTrue(json["Nationality"].Value<JArray>("People").OfType<JObject>().Any(o => o.Value<string>("Href") == "/person/1" && o.Children().Count() == 1));
            Assert.IsTrue(json["Nationality"].Value<JArray>("People").OfType<JObject>().All(o => o.Value<string>("Href") == "/person/1" || o.Children().Count() > 1));
            Assert.IsTrue(JToken.DeepEquals(json["CountryOfResidence"], json["Nationality"]));
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
        public class Country : IHasIdentity<Country>
        {
            IIdentity IHasIdentity.Id => Id;
            public IIdentity<Country> Id { get; set; }
            public string Code { get => Id?.Value?.ToString(); set => Id = FreeIdentity<Country>.Create(value); }
            public string Description { get; set; }
            public List<Person> People { get; set; } = new List<Person>();
        }
        //public class DefCtorParamA : IEquatable<DefCtorParamA>
        //{
        //    public DefCtorParamA(int b, int? c, string d = "xyz")
        //    {
        //        B = b;
        //        C = c;
        //        D = d;
        //    }
        //    public int B { get; }
        //    public int? C { get; }
        //    public string D { get; }
        //    public override int GetHashCode()
        //        => B;
        //    public override bool Equals(object obj)
        //        => obj is DefCtorParamA a ? Equals(a) : false;
        //    public bool Equals(DefCtorParamA other)
        //        => B == other.B && C == other.C && D == other.D;
        //    public static bool operator ==(DefCtorParamA a, DefCtorParamA b)
        //        => a.Equals(b);
        //    public static bool operator !=(DefCtorParamA a, DefCtorParamA b)
        //        => !a.Equals(b);
        //}
    }
}
