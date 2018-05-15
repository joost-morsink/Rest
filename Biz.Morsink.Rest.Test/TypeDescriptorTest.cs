using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Test.Helpers;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var schema = tdc.GetDescriptor(typeof(FSharp.Tryout.Union));
            Assert.IsNotNull(schema);
            Assert.IsInstanceOfType(schema, typeof(TypeDescriptor.Union));
            var u = (TypeDescriptor.Union)schema;
            Assert.AreEqual(4, u.Options.Count);
            var expected = new TypeDescriptor.Union($"{typeof(FSharp.Tryout.Union).Namespace}.{nameof(FSharp.Tryout.Union)}", new[]
            {
                new TypeDescriptor.Record("A", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag",TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "A"), true),
                    new PropertyDescriptor<TypeDescriptor>("A",TypeDescriptor.Primitive.Numeric.Integral.Instance, true)
                }),
                new TypeDescriptor.Record("B", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag",TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "B"), true),
                    new PropertyDescriptor<TypeDescriptor>("B",TypeDescriptor.Primitive.String.Instance, true)
                }),
                new TypeDescriptor.Record("C", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag",TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "C"), true),
                    new PropertyDescriptor<TypeDescriptor>("C",TypeDescriptor.Primitive.Numeric.Float.Instance, true)
                }),
                new TypeDescriptor.Record("D", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag", TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "D"), true)
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
        [TestMethod]
        public void TypeDescriptor_FSharpOption()
        {
            var tdc = new TypeDescriptorCreator();
            var stringOption = tdc.GetDescriptor(typeof(FSharpOption<string>));
            var u = stringOption as TypeDescriptor.Union;
            Assert.IsNotNull(u);
            Assert.AreEqual(2, u.Options.Count);
            Assert.IsTrue(u.Options.OfType<TypeDescriptor.Null>().Any());
            Assert.IsTrue(u.Options.OfType<TypeDescriptor.Primitive.String>().Any());
        }
        [TestMethod]
        public void TypeDescriptor_FSharpList()
        {
            var tdc = new TypeDescriptorCreator();
            var stringList = tdc.GetDescriptor(typeof(Microsoft.FSharp.Collections.FSharpList<string>));
            var a = stringList as TypeDescriptor.Array;
            Assert.IsNotNull(a);
            Assert.IsInstanceOfType(a.ElementType, typeof(TypeDescriptor.Primitive.String));
        }
    }
}
