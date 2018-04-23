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
            var tdc = new TypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(Person));
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
            var tdc = new TypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(PersonC));
            var expected = new TypeDescriptor.Record("Biz.Morsink.Rest.Test.Helpers.PersonC", new[]
            {
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.Age), new TypeDescriptor.Union("System.Int64?", new TypeDescriptor[] { TypeDescriptor.Primitive.Numeric.Integral.Instance, TypeDescriptor.Null.Instance })),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.FirstName), TypeDescriptor.Primitive.String.Instance, true),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.LastName), TypeDescriptor.Primitive.String.Instance, true)
            });
            Assert.IsTrue(expected.Equals(schema));
        }
        [TestMethod]
        public void TypeDescriptor_FSharpUnion()
        {
            var tdc = new TypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(Biz.Morsink.Rest.FSharp.Tryout.Test));
            Assert.IsNotNull(schema);
            Assert.IsInstanceOfType(schema, typeof(TypeDescriptor.Union));
            var u = (TypeDescriptor.Union)schema;
            Assert.AreEqual(3, u.Options.Count);
            var expected = new TypeDescriptor.Union("Biz.Morsink.Rest.FSharp.Tryout.Test", new[]
            {
                new TypeDescriptor.Record("A", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag",TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "A"), true),
                    new PropertyDescriptor<TypeDescriptor>("Item",TypeDescriptor.Primitive.Numeric.Integral.Instance, true)
                }),
                new TypeDescriptor.Record("B", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag",TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "B"), true),
                    new PropertyDescriptor<TypeDescriptor>("Item",TypeDescriptor.Primitive.String.Instance, true)
                }),
                new TypeDescriptor.Record("C", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag",TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "C"), true),
                    new PropertyDescriptor<TypeDescriptor>("Item",TypeDescriptor.Primitive.Numeric.Float.Instance, true)
                })
            });
            Assert.IsTrue(expected.Equals(schema));
        }
        [TestMethod]
        public void TypeDescriptor_FSharpRecord()
        {
            var tdc = new TypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(FSharp.Tryout.Record));
            var expected = new TypeDescriptor.Record("Biz.Morsink.Rest.FSharp.Tryout.Record", new[]
            {
                new PropertyDescriptor<TypeDescriptor>("a", TypeDescriptor.Primitive.Numeric.Integral.Instance, true),
                new PropertyDescriptor<TypeDescriptor>("b", TypeDescriptor.Primitive.String.Instance, true),
                new PropertyDescriptor<TypeDescriptor>("c", TypeDescriptor.Primitive.Numeric.Float.Instance, true)
            });

            Assert.IsTrue(expected.Equals(schema));
        }
    }
}
