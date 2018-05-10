using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Newtonsoft.Json;
using Biz.Morsink.Rest.FSharp.Tryout;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    [TestClass]
    public class FSharpTest
    {
        [TestMethod]
        public void FSharpJson_UnionSerialize()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(new FSharp.FSharpUnionConverter(typeof(Union)));
            var o = JObject.FromObject(Union.NewA(42), ser);
            Assert.IsNotNull(o["Tag"]);
            Assert.AreEqual("A", o["Tag"].Value<string>());
            Assert.IsNotNull(o["A"]);
            Assert.AreEqual(42, o["A"].Value<int>());
            o = JObject.FromObject(Union.NewB("xxx"), ser);
            Assert.IsNotNull(o["Tag"]);
            Assert.AreEqual("B", o["Tag"].Value<string>());
            Assert.IsNotNull(o["B"]);
            Assert.AreEqual("xxx", o["B"].Value<string>());
            o = JObject.FromObject(Union.NewC(3.14), ser);
            Assert.IsNotNull(o["Tag"]);
            Assert.AreEqual("C", o["Tag"].Value<string>());
            Assert.IsNotNull(o["C"]);
            Assert.AreEqual(3.14, o["C"].Value<double>());
            o = JObject.FromObject(Union.D, ser);
            Assert.IsNotNull(o["Tag"]);
            Assert.AreEqual("D", o["Tag"].Value<string>());
            Assert.AreEqual(1, o.Properties().Count());
        }
        [TestMethod]
        public void FSharpJson_UnionStructSerialize()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(new FSharp.FSharpUnionConverter(typeof(UnionStruct)));
            var o = JObject.FromObject(UnionStruct.NewA(42), ser);
            Assert.IsNotNull(o["Tag"]);
            Assert.AreEqual("A", o["Tag"].Value<string>());
            Assert.IsNotNull(o["A"]);
            Assert.AreEqual(42, o["A"].Value<int>());
            o = JObject.FromObject(UnionStruct.NewB("xxx"), ser);
            Assert.IsNotNull(o["Tag"]);
            Assert.AreEqual("B", o["Tag"].Value<string>());
            Assert.IsNotNull(o["B"]);
            Assert.AreEqual("xxx", o["B"].Value<string>());
            o = JObject.FromObject(UnionStruct.NewC(3.14), ser);
            Assert.IsNotNull(o["Tag"]);
            Assert.AreEqual("C", o["Tag"].Value<string>());
            Assert.IsNotNull(o["C"]);
            Assert.AreEqual(3.14, o["C"].Value<double>());
            o = JObject.FromObject(UnionStruct.D, ser);
            Assert.IsNotNull(o["Tag"]);
            Assert.AreEqual("D", o["Tag"].Value<string>());
            Assert.AreEqual(1, o.Properties().Count());
        }
        [TestMethod]
        public void FSharpJson_UnionDeserialize()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(new FSharp.FSharpUnionConverter(typeof(Union)));
            var o = new JObject(new JProperty("A", 42), new JProperty("Tag", "A"));
            using (var rdr = o.CreateReader())
            {
                var a = ser.Deserialize<Union>(rdr);
                Assert.IsNotNull(a);
                Assert.IsTrue(a.IsA);
                if (a is Union.A aa)
                    Assert.AreEqual(42, aa.a);
                else
                    Assert.Fail("a is not of type Union.A");
            }
        }
        [TestMethod]
        public void FSharpJson_SingleCaseSerialize()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(new FSharp.FSharpUnionConverter(typeof(TaggedString)));
            var o = JObject.FromObject(TaggedString.NewTaggedString("abc"), ser);
            Assert.AreEqual("TaggedString", o["Tag"]?.Value<string>());
            Assert.AreEqual("abc", o["Item"]?.Value<string>());
        }
        [TestMethod]
        public void FSharpJson_SingleCaseDeserialize()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(new FSharp.FSharpUnionConverter(typeof(TaggedString)));
            var o = new JObject(
                new JProperty("Item", "abc"),
                new JProperty("Tag", "TaggedString"));

            using (var rdr = o.CreateReader())
            {
                var actual = ser.Deserialize<TaggedString>(rdr);
                Assert.AreEqual(TaggedString.NewTaggedString("abc"), actual);
            }
        }
        [TestMethod]
        public void FSharpJson_NestedSerialize()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(new FSharp.FSharpUnionConverter(typeof(Address)));
            var o = JObject.FromObject(Person.Create("Joost", "Morsink", new[]
            {
                Address.NewHomeAddress(new AddressData("Mainstreet", 1, "Utrecht")),
                Address.NewMailAddress(new AddressData("PO box", 1234, "Utrecht"))
            }), ser);
            Assert.AreEqual("Joost", o["FirstName"].Value<string>());
            var arr = o["Addresses"] as JArray;
            Assert.IsNotNull(arr);
            Assert.AreEqual(2, arr.Count);
            Assert.AreEqual("HomeAddress", arr[0]["Tag"]?.Value<string>());
            Assert.AreEqual("MailAddress", arr[1]["Tag"]?.Value<string>());

            Assert.AreEqual("Mainstreet", arr[0]["Address"]?["Street"]?.Value<string>());
            Assert.AreEqual(1, arr[0]["Address"]?["HouseNumber"]?.Value<int>());
            Assert.AreEqual("Utrecht", arr[0]["Address"]?["City"]?.Value<string>());
            Assert.AreEqual("PO box", arr[1]["Address"]?["Street"]?.Value<string>());
            Assert.AreEqual(1234, arr[1]["Address"]?["HouseNumber"]?.Value<int>());
            Assert.AreEqual("Utrecht", arr[1]["Address"]?["City"]?.Value<string>());


        }
        [TestMethod]
        public void FSharpJson_NestedDeserialize()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(new FSharp.FSharpUnionConverter(typeof(Address)));
            var o = new JObject(
                new JProperty("FirstName", "Joost"),
                new JProperty("LastName", "Morsink"),
                new JProperty("Addresses", new JArray(
                    new JObject(
                        new JProperty("Tag", "HomeAddress"),
                        new JProperty("Address",
                            new JObject(
                                new JProperty("Street", "Mainstreet"),
                                new JProperty("HouseNumber", 1),
                                new JProperty("City", "Utrecht")))),
                    new JObject(
                        new JProperty("Tag", "MailAddress"),
                        new JProperty("Address",
                            new JObject(
                                new JProperty("Street", "PO box"),
                                new JProperty("HouseNumber", 1234),
                                new JProperty("City", "Utrecht"))))
                    )));
            using (var rdr = o.CreateReader())
            {
                var p = ser.Deserialize<Person>(rdr);
                var expected = Person.Create("Joost", "Morsink", new[]
                {
                    Address.NewHomeAddress(new AddressData("Mainstreet", 1, "Utrecht")),
                    Address.NewMailAddress(new AddressData("PO box", 1234, "Utrecht"))
                });
                Assert.AreEqual(expected, p);
            }
        }
        [TestMethod]
        public void FSharpJson_Recursive()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(new FSharp.FSharpUnionConverter(typeof(Expression)));
            var obj = Expression.NewMul(
                Expression.NewAdd(
                    Expression.NewAdd(
                        Expression.NewValue(1),
                        Expression.NewValue(2)),
                    Expression.NewValue(3)),
                Expression.NewAdd(
                    Expression.NewValue(3),
                    Expression.NewValue(4)));
            var json = mul(
                add(
                    add(val(1), val(2)),
                    val(3)),
                add(val(3), val(4)));

            Assert.IsTrue(JToken.DeepEquals(json, JObject.FromObject(obj, ser)));

            using (var rdr = json.CreateReader())
                Assert.AreEqual(obj, ser.Deserialize<Expression>(rdr));
            return;
            
            // Local functions:
            JObject val(int x) => new JObject(
                new JProperty("Tag", "Value"),
                new JProperty("Value", x));
            JObject bin(string type, JObject left, JObject right) => new JObject(
                new JProperty("Tag", type),
                new JProperty("Left", left),
                new JProperty("Right", right));
            JObject add(JObject left, JObject right) => bin("Add", left, right);
            JObject mul(JObject left, JObject right) => bin("Mul", left, right);
        }
    }
}
