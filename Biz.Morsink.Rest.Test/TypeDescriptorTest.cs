using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Test
{
    [TestClass]
    public class TypeDescriptorTest
    {
        [TestMethod]
        public void TypeDescriptor_Happy()
        {
            var schema = typeof(Person).GetDescriptor();
            var expected = new TypeDescriptor.Record("Biz.Morsink.Rest.Test.Helpers.Person", new[]
            {
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.Age), TypeDescriptor.Primitive.Numeric.Integral.Instance),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.FirstName), TypeDescriptor.Primitive.String.Instance),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.LastName),TypeDescriptor.Primitive.String.Instance)
            });
            Assert.AreEqual(expected, schema);
        }
        [TestMethod]
        public void TypeDescriptor_HappyConstructor()
        {
            var schema = typeof(PersonC).GetDescriptor();
            var expected = new TypeDescriptor.Record("Biz.Morsink.Rest.Test.Helpers.PersonC", new[]
            {
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.Age), new TypeDescriptor.Union("System.Int64?", new TypeDescriptor[] { TypeDescriptor.Primitive.Numeric.Integral.Instance, TypeDescriptor.Null.Instance })),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.FirstName), TypeDescriptor.Primitive.String.Instance, true),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.LastName), TypeDescriptor.Primitive.String.Instance, true)
            });
            Assert.IsTrue(expected.Equals(schema));
        }
    }
}
