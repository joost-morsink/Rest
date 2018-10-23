using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using Biz.Morsink.Rest.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Test
{
    [TestClass]
    public class SchemaValidationTest
    {
        [TestMethod]
        public void SchemaVal_Primitives()
        {
            Assert.IsFalse(new SValue(42).Validate(TypeDescriptor.MakeIntegral()).Any());
            Assert.IsFalse(new SValue("abc").Validate(TypeDescriptor.MakeString()).Any());
            Assert.IsFalse(new SValue(DateTime.UtcNow).Validate(TypeDescriptor.MakeDateTime()).Any());
            Assert.IsFalse(new SValue("42").Validate(TypeDescriptor.MakeIntegral()).Any());
            var msgs = new SValue("abc").Validate(TypeDescriptor.MakeIntegral()).ToArray();
            Assert.AreEqual(1, msgs.Length);
            Assert.IsNull(msgs[0].Path);
            Assert.AreEqual(SValidation.Error.NumericValueExpected, msgs[0].Error);
        }
        [TestMethod]
        public void SchemaVal_Record()
        {
            TypeDescriptor td = PersonTypeDescriptor;

            var rec = ValidPerson;

            Assert.IsFalse(rec.Validate(td).Any());
            rec = new SObject(
                new SProperty("Hobbies", new SArray(new SValue(1), SValue.Null)));
            var msgs = rec.Validate(td).ToDictionary(x => x.Path, x => x.Error);
            Assert.AreEqual(3, msgs.Count);
            Assert.IsTrue(msgs.TryGetValue("FirstName", out var err) && err == SValidation.Error.RequiredPropertyMissing);
            Assert.IsTrue(msgs.TryGetValue("LastName", out err) && err == SValidation.Error.RequiredPropertyMissing);
            Assert.IsTrue(msgs.TryGetValue("Hobbies[1]", out err) && err == SValidation.Error.StringValueExpected);

            rec = new SObject(ValidPerson.Properties.Select(p => p.Name == "Id" ? new SProperty("Id", new SValue(1)) : p));
            msgs = rec.Validate(td).ToDictionary(x => x.Path, x => x.Error);
            Assert.AreEqual(1, msgs.Count);
            Assert.IsTrue(msgs.TryGetValue("Id", out err) && err == SValidation.Error.ObjectExpected);

            rec = new SObject(ValidPerson.Properties.Select(p => p.Name == "Id" ? new SProperty("Id", new SObject()) : p));
            msgs = rec.Validate(td).ToDictionary(x => x.Path, x => x.Error);
            Assert.AreEqual(1, msgs.Count);
            Assert.IsTrue(msgs.TryGetValue("Id.Href", out err) && err == SValidation.Error.RequiredPropertyMissing);

            msgs = SValue.Null.Validate(td).ToDictionary(x => x.Path ?? "", x => x.Error);
            Assert.AreEqual(1, msgs.Count);
            Assert.IsTrue(msgs.TryGetValue("", out err) && err == SValidation.Error.ObjectExpected);

        }
        [TestMethod]
        public void SchemaVal_Null()
        {
            Assert.IsFalse(SValue.Null.Validate(TypeDescriptor.MakeNull()).Any());
            Assert.IsTrue(new SValue(1).Validate(TypeDescriptor.MakeNull()).Any());
            Assert.IsTrue(SValue.Null.Validate(TypeDescriptor.MakeString()).Any());
        }
        [TestMethod]
        public void SchemaVal_Intersection()
        {
            var required = TypeDescriptor.MakeRecord("req", PersonTypeDescriptor.Properties.Where(p => p.Value.Required).Select(p => p.Value),null);
            var notRequired = TypeDescriptor.MakeRecord("nonreq", PersonTypeDescriptor.Properties.Where(p => !p.Value.Required).Select(p => p.Value), null);
            var intersection = TypeDescriptor.MakeIntersection("int", new[] { required, notRequired }, null);
            Assert.IsFalse(ValidPerson.Validate(intersection).Any());

            var rec = new SObject(
                new SProperty("Hobbies", new SArray(new SValue(1), SValue.Null)));
            var msgs = rec.Validate(intersection).ToDictionary(x => x.Path, x => x.Error);
            Assert.AreEqual(3, msgs.Count);
            Assert.IsTrue(msgs.TryGetValue("FirstName", out var err) && err == SValidation.Error.RequiredPropertyMissing);
            Assert.IsTrue(msgs.TryGetValue("LastName", out err) && err == SValidation.Error.RequiredPropertyMissing);
            Assert.IsTrue(msgs.TryGetValue("Hobbies[1]", out err) && err == SValidation.Error.StringValueExpected);

        }
        [TestMethod]
        public void SchemaVal_Union()
        {
            var union = TypeDescriptor.MakeUnion("union", new[]
            {
                PersonTypeDescriptor,
                TypeDescriptor.MakeIntegral()
            }, null);
            Assert.IsFalse(ValidPerson.Validate(union).Any());
            Assert.IsFalse(new SValue(1).Validate(union).Any());
            var msgs = SValue.Null.Validate(union).ToLookup(x => x.Path ?? "", x => x.Error);
            Assert.AreEqual(1, msgs.Count);
            Assert.AreEqual(3, msgs[""].Count());
            Assert.IsTrue(new[] { SValidation.Error.NoOptionMatch, SValidation.Error.ObjectExpected, SValidation.Error.NumericValueExpected }
                .All(msgs[""].Contains));
        }
        [TestMethod]
        public void SchemaVal_Array()
        {
            var arr = new SArray(new SValue(1), new SValue("abc"), new SValue("23"), ValidPerson);
            var msgs = arr.Validate(TypeDescriptor.MakeArray(TypeDescriptor.MakeIntegral())).ToDictionary(x => x.Path, x => x.Error);
            Assert.AreEqual(2, msgs.Count);
            Assert.IsTrue(msgs.TryGetValue("[1]", out var err) && err == SValidation.Error.NumericValueExpected);
            Assert.IsTrue(msgs.TryGetValue("[3]", out err) && err == SValidation.Error.ValueExpected);
        }

        private static SObject ValidPerson
        { get; } = new SObject(
                            new SProperty("Id", new SObject(
                                new SProperty("Href", new SValue("http://localhost:5000/person/1")))),
                            new SProperty("FirstName", new SValue("Joost")),
                            new SProperty("LastName", new SValue("Morsink")),
                            new SProperty("Age", new SValue(39)),
                            new SProperty("Hobbies", new SArray(new SValue("Programming"), new SValue("Playing guitar"))),
                            new SProperty("Dummy", SValue.Null));

        public static TypeDescriptor.Record PersonTypeDescriptor
        { get; } = new TypeDescriptor.Record("rec", new[]
            {
                new PropertyDescriptor<TypeDescriptor>("Id", TypeDescriptor.MakeRecord("id",new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Href", TypeDescriptor.MakeString(), true)
                }, null), false),
                new PropertyDescriptor<TypeDescriptor>("FirstName", TypeDescriptor.MakeString(), true),
                new PropertyDescriptor<TypeDescriptor>("LastName", TypeDescriptor.MakeString(), true),
                new PropertyDescriptor<TypeDescriptor>("Age", TypeDescriptor.MakeIntegral(), false),
                new PropertyDescriptor<TypeDescriptor>("Hobbies", TypeDescriptor.MakeArray(TypeDescriptor.MakeString()), false)
            }, null);

    }

}
