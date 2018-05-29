using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    [TestClass]
    public class SemanticStructTest
    {
        public struct EmailAddress
        {
            public EmailAddress(string address)
            {
                Address = address;
            }

            public string Address { get; }
        }

        [TestMethod]
        public void SemStr_Serialize()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(SemanticStructConverter.Create<EmailAddress>());
            var o = JToken.FromObject(new EmailAddress("info@test.nl"), ser);
            if (o is JValue v)
            {
                Assert.AreEqual("info@test.nl", v.Value<string>());
            }
            else
                Assert.Fail("Not a JValue");
        }
        [TestMethod]
        public void SemStr_Deserialize()
        {
            var ser = new JsonSerializer();
            ser.Converters.Add(SemanticStructConverter.Create<EmailAddress>());
            var o = new JValue("info@test.nl");
            var e = o.ToObject<EmailAddress>(ser);
            Assert.AreEqual("info@test.nl", e.Address);
        }
    }
}
