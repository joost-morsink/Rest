﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
