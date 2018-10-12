using Biz.Morsink.Identity;
using Biz.Morsink.Rest.FSharp.Tryout;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using Biz.Morsink.Rest.Test.Helpers;
using Microsoft.FSharp.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Test
{
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
            var actual = serializer.Serialize(NewContext(), Tuple.Create(p,c));
            Assert.AreEqual(expected, actual);
            var back = serializer.Deserialize<Tuple<Car,Helpers.Person>>(NewContext(), actual);
            Assert.IsNotNull(back);
            Assert.AreEqual("Joost", back.Item2.FirstName);
            Assert.AreEqual("Morsink", back.Item2.LastName);
            Assert.AreEqual(38, back.Item2.Age);
            Assert.AreEqual("Volvo", back.Item1.Brand);
            Assert.AreEqual("V40", back.Item1.Model);

        }
    
    }
}
