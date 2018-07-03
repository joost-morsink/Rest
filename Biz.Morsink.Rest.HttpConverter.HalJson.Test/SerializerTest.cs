using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.DataConvert;
using System;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace Biz.Morsink.Rest.HttpConverter.HalJson.Test
{
    [TestClass]
    public class SerializerTest
    {
        public class HelperA
        {
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
        }
        public class HelperB
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
        }
        public class HelperC
        {
            public string A { get; set; }
            public HelperA[] As { get; set; }
            public List<HelperB> Bs { get; set; }
            public IReadOnlyCollection<HelperA> MoreAs { get; set; }
        }

        private HalSerializer serializer;
        private Func<HalContext> context;

        [TestInitialize]
        public void Init()
        {
            var typereps = Enumerable.Empty<ITypeRepresentation>();
            var converter = DataConverter.Default;
            var tdc = new TypeDescriptorCreator(typereps);
            serializer = new HalSerializer(tdc, converter, typereps);
            context = new Func<HalContext>(HalContext.Create);
        }
        [TestMethod]
        public void HalSerializer_Primitives()
        {
            Assert.AreEqual("123", serializer.Serialize(context(), 123).ToString());
            Assert.AreEqual("xyz", ((JValue)serializer.Serialize(context(), "xyz")).Value);
            Assert.AreEqual("2018-07-03T07:53:00.000Z", ((JValue)serializer.Serialize(context(), new DateTime(2018, 7, 3, 7, 53, 0, DateTimeKind.Utc))).ToString());
            Assert.AreEqual("12.340", ((JValue)serializer.Serialize(context(), 12.340m)).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("12.34", ((JValue)serializer.Serialize(context(), 12.340)).ToString(CultureInfo.InvariantCulture));
        }
        [TestMethod]
        public void HalSerializer_PrimitivesBack()
        {
            Assert.AreEqual(123, serializer.Deserialize<int>(context(), JToken.Parse("123")));
            Assert.AreEqual("xyz", serializer.Deserialize<string>(context(), JToken.Parse("\"xyz\"")));
            Assert.AreEqual(new DateTime(2018, 7, 3, 7, 53, 0, DateTimeKind.Utc), serializer.Deserialize<DateTime>(context(), JToken.Parse("\"2018-07-03T07:53:00.000Z\"")));
            Assert.AreEqual(12.34m, serializer.Deserialize<decimal>(context(), JToken.Parse("12.340")));
            Assert.AreEqual(12.34, serializer.Deserialize<double>(context(), JToken.Parse("12.34")), 0.000_001);
        }
        [TestMethod]
        public void HalSerializer_MutableRecords()
        {
            var a = new HelperA { A = "a", B = "b", C = "c" };
            var json = serializer.Serialize(context(), a) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["a"]);
            Assert.IsNotNull(json["b"]);
            Assert.IsNotNull(json["c"]);
            var dea = serializer.Deserialize<HelperA>(context(), json);
            Assert.AreEqual("a", dea.A);
            Assert.AreEqual("b", dea.B);
            Assert.AreEqual("c", dea.C);

        }
        [TestMethod]
        public void HalSerializer_ImmutableRecords()
        {
            var b = new HelperB("a", "b", "c");
            var json = serializer.Serialize(context(), b) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["a"]);
            Assert.IsNotNull(json["b"]);
            Assert.IsNotNull(json["c"]);
            var deb = serializer.Deserialize<HelperB>(context(), json);
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
            var json = serializer.Serialize(context(), c) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(4, json.Properties().Count());
            Assert.IsNotNull(json["a"]);
            Assert.IsNotNull(json["as"]);
            Assert.AreEqual(2, json.Value<JArray>("as").Count);
            Assert.IsNotNull(json["bs"]);
            Assert.AreEqual(2, json.Value<JArray>("bs").Count);
            Assert.IsNotNull(json["moreAs"]);
            Assert.AreEqual(1, json.Value<JArray>("moreAs").Count);

            var dec = serializer.Deserialize<HelperC>(context(), json);
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
            var json = serializer.Serialize(context(),x) as JObject;
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["A"]);
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
        }
    }
}
