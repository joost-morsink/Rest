using Biz.Morsink.Rest.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.Test
{
    [TestClass]
    public class SerializationBinaryTest
    {
        private readonly SBinaryFormatter formatter;

        public SerializationBinaryTest()
        {
            formatter = new SBinaryFormatter();
        }
        private async Task<byte[]> Serialize(SItem item)
        {
            using (var ms = new MemoryStream())
            {
                await formatter.WriteItem(ms, item);
                return ms.ToArray();
            }
        }
        private async Task<SItem> Deserialize(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return await formatter.ReadItem(ms);
            }
        }
        private async Task TestPrimitive(object val)
        {
            var sval = new SValue(val);
            await TestItem(sval);
        }
        private async Task TestItem(SItem item)
        {
            var str = await Serialize(item);
            var back = await Deserialize(str);
            Assert.AreEqual(item, back);
        }
        [TestMethod]
        public async Task SerializationBin_Primitives()
        {
            await TestPrimitive("Test string");
            await TestPrimitive((sbyte)-12);
            await TestPrimitive((short)12345);
            await TestPrimitive(42);
            await TestPrimitive(12345678901234567890L);
            await TestPrimitive((byte)123);
            await TestPrimitive((ushort)54321);
            await TestPrimitive((uint)987654321);
            await TestPrimitive(12345678901234567890UL);
            await TestPrimitive(3.14159f);
            await TestPrimitive(3.14159265358979323846);
            await TestPrimitive(123.45m);
            await TestPrimitive(DateTime.UtcNow);
            await TestPrimitive(DateTimeOffset.Now);
        }
        [TestMethod]
        public async Task SerializationBin_Blob() { 
            var bytes = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();
            var str = await Serialize(new SValue(bytes));
            var back = await Deserialize(str);
            if (back is SValue val && val.Value is byte[] blob)
            {
                for (int i = 0; i < 256; i++)
                    Assert.AreEqual(i, blob[i]);
            }
            else
                Assert.Fail();
        }
        [TestMethod]
        public async Task SerializationBin_Object()
        {
            var o = new SObject(
                new SProperty("A", new SValue(456)),
                new SProperty("B", new SValue("def")),
                new SProperty("C", new SObject(
                    new SProperty("D", new SValue(42.00m)))));
            await TestItem(o);
        }
        [TestMethod]
        public async Task SerializationBin_Array()
        {
            var a = new SArray(
                new SValue(123),
                new SValue("abc"),
                new SArray(new SValue((byte)1), new SValue((ushort)2), new SValue((long)3)),
                new SArray(new SValue(3.14159f), new SValue(3.14159)));
            await TestItem(a);
        }
        [TestMethod]
        public async Task SerializationBin_Null()
        {
            await TestPrimitive(null);
        }
    }
}
