using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Biz.Morsink.Rest.HttpConverter.Xml.Test
{
    [TestClass]
    public class SerializerTest
    {
        private XmlSerializer serializer;

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
        [TestInitialize]
        public void Init()
        {
            var typereps = Enumerable.Empty<ITypeRepresentation>();
            var converter = DataConverter.Default;
            var tdc = new TypeDescriptorCreator(typereps);
            serializer = new XmlSerializer(tdc, converter, typereps);
        }
        [TestMethod]
        public void XmlSerializer_Primitives()
        {
            Assert.AreEqual("123", serializer.Serialize(123).Value);
            Assert.AreEqual("xyz", serializer.Serialize("xyz").Value);
            Assert.AreEqual("2017-12-12T12:53:00.000Z", serializer.Serialize(new DateTime(2017, 12, 12, 12, 53, 0, DateTimeKind.Utc)).Value);
            Assert.AreEqual("12.340", serializer.Serialize(12.340m).Value);
            Assert.AreEqual("12.34", serializer.Serialize(12.340).Value);
        }
        [TestMethod]
        public void XmlSerializer_PrimitivesBack()
        {
            Assert.AreEqual(123, serializer.Deserialize<int>(XElement.Parse("<x>123</x>")));
            Assert.AreEqual("xyz", serializer.Deserialize<string>(XElement.Parse("<x>xyz</x>")));
            Assert.AreEqual(new DateTime(2017, 12, 12, 13, 53, 0, DateTimeKind.Utc), serializer.Deserialize<DateTime>(XElement.Parse("<x>2017-12-12T13:53:00.000Z</x>")));
            Assert.AreEqual(12.34m, serializer.Deserialize<decimal>(XElement.Parse("<x>12.340</x>")));
            Assert.AreEqual(12.34, serializer.Deserialize<double>(XElement.Parse("<x>12.34</x>")), 0.000_001);
        }
        [TestMethod]
        public void XmlSerializer_MutableRecords()
        {
            var a = new HelperA { A = "1", B = "2", C = "3" };
            var xml = serializer.Serialize(a);
            Assert.AreEqual(3, xml.Elements().Count());
            Assert.IsNotNull(xml.Element("A"));
            Assert.IsNotNull(xml.Element("B"));
            Assert.IsNotNull(xml.Element("C"));
            var dea = serializer.Deserialize<HelperA>(xml);
            Assert.AreEqual("1", dea.A);
            Assert.AreEqual("2", dea.B);
            Assert.AreEqual("3", dea.C);

        }
        [TestMethod]
        public void XmlSerializer_ImmutableRecords()
        {
            var b = new HelperB("1", "2", "3");
            var xml = serializer.Serialize(b);
            Assert.AreEqual(3, xml.Elements().Count());
            Assert.IsNotNull(xml.Element("A"));
            Assert.IsNotNull(xml.Element("B"));
            Assert.IsNotNull(xml.Element("C"));
            var deb = serializer.Deserialize<HelperB>(xml);
            Assert.AreEqual("1", deb.A);
            Assert.AreEqual("2", deb.B);
            Assert.AreEqual("3", deb.C);
        }
        [TestMethod]
        public void XmlSerializer_Collections()
        {
            var c = new HelperC
            {
                A = "123",
                As = new[] { new HelperA { A = "a", B = "b", C = "c" }, new HelperA { A = "d", B = "e", C = "f" } },
                Bs = new List<HelperB> { new HelperB("A", "B", "C"), new HelperB("X", "Y", "Z") },
                MoreAs = new[] { new HelperA { A = "!", B = "@", C = "#" } }
            };
            var xml = serializer.Serialize(c);
            Assert.AreEqual(4, xml.Elements().Count());
            Assert.IsNotNull(xml.Element("A"));
            Assert.IsNotNull(xml.Element("As"));
            Assert.AreEqual(2, xml.Element("As").Elements().Count());
            Assert.IsNotNull(xml.Element("Bs"));
            Assert.AreEqual(2, xml.Element("Bs").Elements().Count());
            Assert.IsNotNull(xml.Element("MoreAs"));
            Assert.AreEqual(1, xml.Element("MoreAs").Elements().Count());

            var dec = serializer.Deserialize<HelperC>(xml);
            Assert.AreEqual("123", dec.A);
            Assert.AreEqual(2, dec.As.Length);
            Assert.AreEqual("e", dec.As[1].B);
            Assert.AreEqual(2, dec.Bs.Count);
            Assert.AreEqual("Z", dec.Bs[1].C);
            Assert.AreEqual(1, dec.MoreAs.Count);
            Assert.AreEqual("!", dec.MoreAs.First().A);
        }
        [TestMethod]
        public void XmlSerializer_Dictionaries()
        {
            var x = new Dictionary<string, object>
            {
                ["A"] = 1,
                ["B"] = "abc",
                ["C"] = DateTime.UtcNow
            };
            var xml = serializer.Serialize(x);
            Assert.AreEqual(3, xml.Elements().Count());
            Assert.IsNotNull(xml.Element("A"));
            Assert.IsNotNull(xml.Element("B"));
            Assert.IsNotNull(xml.Element("C"));

            xml = XElement.Parse("<a><a>1</a><b><c>2</c><d>3</d></b></a>");
            var dict = serializer.Deserialize<Dictionary<string, object>>(xml);
            Assert.IsTrue(dict.TryGetValue("a",out var y));
            Assert.AreEqual("1", y);
            Assert.IsTrue(dict.TryGetValue("b", out y));
            if (y is Dictionary<string, object> b)
            {
                Assert.IsTrue(b.TryGetValue("c", out y));
                Assert.AreEqual("2", y);
                Assert.IsTrue(b.TryGetValue("d", out y));
                Assert.AreEqual("3", y);
            }
            else
                Assert.Fail();
        }
    }
}
