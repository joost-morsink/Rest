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
        public void FSharpJson_Union()
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
        public void FSharpJson_UnionStruct()
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
    }
}
