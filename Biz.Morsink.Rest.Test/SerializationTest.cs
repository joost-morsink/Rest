using Biz.Morsink.Identity;
using Biz.Morsink.Rest.FSharp.Tryout;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using Biz.Morsink.Rest.Test.Helpers;
using Microsoft.FSharp.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Test
{
    using UPC = UnionRepresentation<Helpers.Person, Car>;

    [TestClass]
    public class SerializationTest
    {
        private ITypeDescriptorCreator typeDescriptorCreator;
        private Serializer<SerializationContext> serializer;

        private SerializationContext NewContext()
            => SerializationContext.Create(new FreeIdentityProvider());
        [TestInitialize]
        public void Init()
        {
            typeDescriptorCreator = new StandardTypeDescriptorCreator(new[] { TestIdentityRepresentation.Instance, TupleAsIntersectionRepresentation.Instance });
            serializer = new Serializer<SerializationContext>(typeDescriptorCreator);
        }
        [TestMethod]
        public void Serializer_Primitives()
        {
            Assert.AreEqual(new SValue(42), serializer.Serialize(NewContext(), 42));
            Assert.AreEqual(new SValue(123L), serializer.Serialize(NewContext(), 123L));
            Assert.AreEqual(new SValue("Abc"), serializer.Serialize(NewContext(), "Abc"));
            Assert.AreEqual(new SValue(new DateTime(2018, 8, 14, 13, 56, 0, DateTimeKind.Utc)), serializer.Serialize(NewContext(), new DateTime(2018, 8, 14, 13, 56, 0, DateTimeKind.Utc)));
            Assert.AreEqual(new SValue(123.45m), serializer.Serialize(NewContext(), 123.45m));
        }
        [TestMethod]
        public void Serializer_Record()
        {
            var p = new Helpers.Person { FirstName = "Joost", LastName = "Morsink", Age = 38 };
            var expected = new SObject(
                new SProperty("FirstName", new SValue("Joost")),
                new SProperty("LastName", new SValue("Morsink")),
                new SProperty("Age", new SValue(38)));
            var actual = serializer.Serialize(NewContext(), p);
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<Helpers.Person>(NewContext(), actual);
            Assert.IsNotNull(back);
            Assert.AreEqual("Joost", back.FirstName);
            Assert.AreEqual("Morsink", back.LastName);
            Assert.AreEqual(38, back.Age);
        }
        [TestMethod]
        public void Serializer_RecordC()
        {
            var p = new PersonC("Joost", "Morsink", 38);
            var expected = new SObject(
                new SProperty("FirstName", new SValue("Joost")),
                new SProperty("LastName", new SValue("Morsink")),
                new SProperty("Age", new SValue(38)));
            var actual = serializer.Serialize(NewContext(), p);
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<PersonC>(NewContext(), actual);
            Assert.IsNotNull(back);
            Assert.AreEqual("Joost", back.FirstName);
            Assert.AreEqual("Morsink", back.LastName);
            Assert.AreEqual(38, back.Age);
        }
        [TestMethod]
        public void Serializer_RecordMixed()
        {
            var p = new PersonM("Joost", "Morsink") { Age = 39 };
            var expected = new SObject(
                new SProperty("FirstName", new SValue("Joost")),
                new SProperty("LastName", new SValue("Morsink")),
                new SProperty("Age", new SValue(39)));
            var actual = serializer.Serialize(NewContext(), p);
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<PersonM>(NewContext(), actual);
            Assert.IsNotNull(back);
            Assert.AreEqual("Joost", back.FirstName);
            Assert.AreEqual("Morsink", back.LastName);
            Assert.AreEqual(39, back.Age);
        }
        [TestMethod]
        public void Serializer_MissingProp()
        {
            var p = new Helpers.Person { FirstName = "Joost", Age = 39 };
            var expected = new SObject(
                new SProperty("FirstName", new SValue("Joost")),
                new SProperty("LastName", SValue.Null),
                new SProperty("Age", new SValue(39)));
            var actual = serializer.Serialize(NewContext(), p);
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<Helpers.Person>(NewContext(), actual);
            Assert.IsNotNull(back);
            Assert.AreEqual("Joost", back.FirstName);
            Assert.IsNull(back.LastName);
            Assert.AreEqual(39, back.Age);

        }
        [TestMethod]
        public void Serializer_FsRecord()
        {
            var fsp = new FSharp.Tryout.Person("Joost", "Morsink", FSharpList<Address>.Cons(Address.NewHomeAddress(new AddressData("Teststreet", 123, "Utrecht")), FSharpList<Address>.Empty));
            var expected = new SObject(
                new SProperty("FirstName", new SValue("Joost")),
                new SProperty("LastName", new SValue("Morsink")),
                new SProperty("Addresses", new SArray(new SObject(
                    new SProperty("Tag", new SValue("HomeAddress")),
                    new SProperty("Address", new SObject(
                        new SProperty("Street", new SValue("Teststreet")),
                        new SProperty("HouseNumber", new SValue(123)),
                        new SProperty("City", new SValue("Utrecht"))))))));
            var actual = serializer.Serialize(NewContext(), fsp);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void Serializer_Intersection()
        {
            var p = new Helpers.Person { FirstName = "Joost", LastName = "Morsink", Age = 38 };
            var c = new Car { Brand = "Volvo", Model = "V40" };
            var expected = new SObject(
                new SProperty("FirstName", new SValue("Joost")),
                new SProperty("LastName", new SValue("Morsink")),
                new SProperty("Age", new SValue(38)),
                new SProperty("Brand", new SValue("Volvo")),
                new SProperty("Model", new SValue("V40")));
            var actual = serializer.Serialize(NewContext(), Tuple.Create(p, c));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<Tuple<Car, Helpers.Person>>(NewContext(), actual);
            Assert.IsNotNull(back);
            Assert.AreEqual("Joost", back.Item2.FirstName);
            Assert.AreEqual("Morsink", back.Item2.LastName);
            Assert.AreEqual(38, back.Item2.Age);
            Assert.AreEqual("Volvo", back.Item1.Brand);
            Assert.AreEqual("V40", back.Item1.Model);

        }
        [TestMethod]
        public void Serializer_Union()
        {
            var p = new Helpers.Person { FirstName = "Joost", LastName = "Morsink", Age = 39 };
            var c = new Helpers.Car { Brand = "Volvo", Model = "V40" };
            var expected = new SObject(
               new SProperty("FirstName", new SValue("Joost")),
               new SProperty("LastName", new SValue("Morsink")),
               new SProperty("Age", new SValue(39)));
            var actual = serializer.Serialize(NewContext(), new UPC.Option1(p));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<UPC>(NewContext(), actual);
            if (!(back is UPC.Option1 backP))
            {
                Assert.Fail("Not a person"); return;
            }
            Assert.IsNotNull(backP.Item);
            Assert.AreEqual("Joost", backP.Item.FirstName);
            Assert.AreEqual("Morsink", backP.Item.LastName);
            Assert.AreEqual(39, backP.Item.Age);

            expected = new SObject(
                new SProperty("Brand", new SValue("Volvo")),
                new SProperty("Model", new SValue("V40")));
            actual = serializer.Serialize(NewContext(), new UPC.Option2(c));
            back = serializer.Deserialize<UPC>(NewContext(), actual);
            if (!(back is UPC.Option2 backC))
            {
                Assert.Fail("Not a car"); return;
            }
            Assert.IsNotNull(backC.Item);
            Assert.AreEqual("Volvo", backC.Item.Brand);
            Assert.AreEqual("V40", backC.Item.Model);
        }
        [TestMethod]
        public void Serializer_Dict()
        {
            var x = new Dictionary<string, object>
            {
                ["A"] = 1,
                ["B"] = "abc",
                ["C"] = null
            };
            var actual = serializer.Serialize(NewContext(), x);
            var expected = new SObject(
                new SProperty("A", new SValue(1)),
                new SProperty("B", new SValue("abc")),
                new SProperty("C", SValue.Null));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<Dictionary<string, object>>(NewContext(), actual);
            Assert.AreEqual(3, back.Count);
            Assert.IsTrue(new[] { "A", "B", "C" }.All(back.ContainsKey));
        }
        [TestMethod]
        public void Serializer_SortedDict()
        {
            var x = new SortedDictionary<string, object>
            {
                ["A"] = 1,
                ["B"] = "abc",
                ["C"] = null
            };
            var actual = serializer.Serialize(NewContext(), x);
            var expected = new SObject(
                new SProperty("A", new SValue(1)),
                new SProperty("B", new SValue("abc")),
                new SProperty("C", SValue.Null));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<SortedDictionary<string, object>>(NewContext(), actual);
            Assert.AreEqual(3, back.Count);
            Assert.IsTrue(new[] { "A", "B", "C" }.All(back.ContainsKey));
        }
        [TestMethod]
        public void Serializer_ImmDict()
        {
            var x = ImmutableDictionary<string, object>.Empty
                .Add("A", 1)
                .Add("B", "abc")
                .Add("C", null);

            var actual = serializer.Serialize(NewContext(), x);
            var expected = new SObject(
                new SProperty("A", new SValue(1)),
                new SProperty("B", new SValue("abc")),
                new SProperty("C", SValue.Null));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<ImmutableDictionary<string, object>>(NewContext(), actual);
            Assert.AreEqual(3, back.Count);
            Assert.IsTrue(new[] { "A", "B", "C" }.All(back.ContainsKey));
        }
        [TestMethod]
        public void Serializer_ReadonlyDict()
        {
            var dict = new Dictionary<string, string>
            {
                ["A"] = "abc",
                ["B"] = "def",
                ["C"] = "ghi"
            };
            var actual = serializer.Serialize<IReadOnlyDictionary<string, string>>(NewContext(), dict);
            var expected = new SObject(
                new SProperty("A", new SValue("abc")),
                new SProperty("B", new SValue("def")),
                new SProperty("C", new SValue("ghi")));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<IReadOnlyDictionary<string, string>>(NewContext(), actual);
            Assert.IsNotNull(back);
            Assert.AreEqual(3, back.Count);
            Assert.IsTrue(dict.Keys.All(back.ContainsKey));
        }
        public struct EmailAddress
        {
            public EmailAddress(string address)
            {
                Address = address;
            }

            public string Address { get; }
        }
        [TestMethod]
        public void Serializer_SemStr()
        {
            const string address = "info@example.com";
            var addr = new EmailAddress(address);
            var actual = serializer.Serialize(NewContext(), address);
            var expected = new SValue(address);
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<EmailAddress>(NewContext(), actual);
            Assert.AreEqual(address, back.Address);
        }
        [TestMethod]
        public void Serializer_ImmList()
        {
            var x = ImmutableList<int>.Empty.Add(1).Add(2).Add(3);
            var actual = serializer.Serialize(NewContext(), x);
            var expected = new SArray(new SValue(1), new SValue(2), new SValue(3));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<ImmutableList<int>>(NewContext(), actual);
            Assert.AreEqual(3, back.Count);
            Assert.AreEqual(1, back[0]);
            Assert.AreEqual(2, back[1]);
            Assert.AreEqual(3, back[2]);
        }
        [TestMethod]
        public void Serializer_ImmArray()
        {
            var x = ImmutableArray<int>.Empty.Add(1).Add(2).Add(3);
            var actual = serializer.Serialize(NewContext(), x);
            var expected = new SArray(new SValue(1), new SValue(2), new SValue(3));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<ImmutableArray<int>>(NewContext(), actual);
            Assert.AreEqual(3, back.Length);
            Assert.AreEqual(1, back[0]);
            Assert.AreEqual(2, back[1]);
            Assert.AreEqual(3, back[2]);
        }
        [TestMethod]
        public void Serializer_ImmStack()
        {
            var x = ImmutableStack<int>.Empty.Push(1).Push(2).Push(3);
            var actual = serializer.Serialize(NewContext(), x);
            var expected = new SArray(new SValue(3), new SValue(2), new SValue(1));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<ImmutableStack<int>>(NewContext(), actual);
            Assert.AreEqual(3, back.Count());
            Assert.AreEqual(6, back.Sum());
            Assert.AreEqual(x.Peek(), back.Peek());
        }
        [TestMethod]
        public void Serializer_ImmQueue()
        {
            var x = ImmutableQueue<int>.Empty.Enqueue(1).Enqueue(2).Enqueue(3);
            var actual = serializer.Serialize(NewContext(), x);
            var expected = new SArray(new SValue(1), new SValue(2), new SValue(3));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<ImmutableQueue<int>>(NewContext(), actual);
            Assert.AreEqual(3, back.Count());
            Assert.AreEqual(6, back.Sum());
        }
        [TestMethod]
        public void Serializer_ImmHashSet()
        {
            var x = ImmutableHashSet<int>.Empty.Add(1).Add(2).Add(3);
            var actual = serializer.Serialize(NewContext(), x);
            var expected = new SArray(new SValue(1), new SValue(2), new SValue(3));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<ImmutableHashSet<int>>(NewContext(), actual);
            Assert.AreEqual(3, back.Count);
            Assert.IsTrue(new[] { 1, 2, 3 }.All(back.Contains));
        }
    }
}
