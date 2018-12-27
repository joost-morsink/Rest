using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Test.Helpers;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Test
{
    [TestClass]
    public partial class TypeDescriptorTest
    {
        private static readonly TypeDescriptor personDescriptor = new TypeDescriptor.Record("Biz.Morsink.Rest.Test.Helpers.Person", new[]
            {
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.Age), TypeDescriptor.Primitive.Numeric.Integral.Instance),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.FirstName), TypeDescriptor.Primitive.String.Instance),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.LastName),TypeDescriptor.Primitive.String.Instance)
            }, null);
        [TestMethod]
        public void TypeDescriptor_Happy()
        {
            var tdc = new StandardTypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(Person));
            var expected = personDescriptor;
            Assert.AreEqual(expected, schema);
        }
        [TestMethod]
        public void TypeDescriptor_HappyConstructor()
        {
            var tdc = new StandardTypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(PersonC));
            var expected = new TypeDescriptor.Record("Biz.Morsink.Rest.Test.Helpers.PersonC", new[]
            {
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.Age), new TypeDescriptor.Union("System.Int64?", new TypeDescriptor[] { TypeDescriptor.Primitive.Numeric.Integral.Instance, TypeDescriptor.Null.Instance },null)),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.FirstName), TypeDescriptor.Primitive.String.Instance, true),
                new PropertyDescriptor<TypeDescriptor>(nameof(Person.LastName), TypeDescriptor.Primitive.String.Instance, true)
            }, null);
            Assert.IsTrue(expected.Equals(schema));
        }
        [TestMethod]
        public void TypeDescriptor_List()
        {
            var tdc = new StandardTypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(List<Person>));
            var expected = new TypeDescriptor.Array(new TypeDescriptor.Reference($"{typeof(Person).Namespace}.{typeof(Person).Name}"));
            Assert.AreEqual(expected, schema);
        }
        [TestMethod]
        public void TypeDescriptor_Dictionary()
        {
            var tdc = new StandardTypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(Dictionary<string, Person>));
            var expected = new TypeDescriptor.Dictionary("", personDescriptor);
            Assert.AreEqual(expected, schema);
            schema = tdc.GetDescriptor(typeof(Dictionary<string, object>));
            expected = new TypeDescriptor.Dictionary("", TypeDescriptor.MakeAny());
            Assert.AreEqual(expected, schema);
        }
        [TestMethod]
        public void TypeDescriptor_FSharpUnion()
        {
            var tdc = new StandardTypeDescriptorCreator();
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
                },null),
                new TypeDescriptor.Record("B", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag",TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "B"), true),
                    new PropertyDescriptor<TypeDescriptor>("B",TypeDescriptor.Primitive.String.Instance, true)
                },null),
                new TypeDescriptor.Record("C", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag",TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "C"), true),
                    new PropertyDescriptor<TypeDescriptor>("C",TypeDescriptor.Primitive.Numeric.Float.Instance, true)
                },null),
                new TypeDescriptor.Record("D", new []
                {
                    new PropertyDescriptor<TypeDescriptor>("Tag", TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, "D"), true)
                },null)
            }, null);
            Assert.IsTrue(expected.Equals(schema));
        }
        [TestMethod]
        public void TypeDescriptor_FSharpSingleCaseUnion()
        {
            var tdc = new StandardTypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(FSharp.Tryout.TaggedString));
            Assert.IsNotNull(schema);
            Assert.IsInstanceOfType(schema, typeof(TypeDescriptor.Primitive.String));
        }
        [TestMethod]
        public void TypeDescriptor_FSharpRecord()
        {
            var tdc = new StandardTypeDescriptorCreator();
            var schema = tdc.GetDescriptor(typeof(FSharp.Tryout.Record));
            var expected = new TypeDescriptor.Record("Biz.Morsink.Rest.FSharp.Tryout.Record", new[]
            {
                new PropertyDescriptor<TypeDescriptor>("a", TypeDescriptor.Primitive.Numeric.Integral.Instance, true),
                new PropertyDescriptor<TypeDescriptor>("b", TypeDescriptor.Primitive.String.Instance, true),
                new PropertyDescriptor<TypeDescriptor>("c", TypeDescriptor.Primitive.Numeric.Float.Instance, true)
            }, null);

            Assert.IsTrue(expected.Equals(schema));
        }
        [TestMethod]
        public void TypeDescriptor_FSharpOption()
        {
            var tdc = new StandardTypeDescriptorCreator();
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
            var tdc = new StandardTypeDescriptorCreator();
            var stringList = tdc.GetDescriptor(typeof(Microsoft.FSharp.Collections.FSharpList<string>));
            var a = stringList as TypeDescriptor.Array;
            Assert.IsNotNull(a);
            Assert.IsInstanceOfType(a.ElementType, typeof(TypeDescriptor.Primitive.String));
        }
        [TestMethod]
        public void TypeDescriptor_SemanticStruct()
        {
            var tdc = new StandardTypeDescriptorCreator();
            var email = tdc.GetDescriptor(typeof(EmailAddress));
            Assert.IsNotNull(email);
            Assert.IsInstanceOfType(email, typeof(TypeDescriptor.Primitive.String));
        }
        [TestMethod]
        public void TypeDescriptor_Representation()
        {
            var tdc = new StandardTypeDescriptorCreator(new[] { TestIdentityRepresentation.Instance });
            var identity = tdc.GetDescriptor(typeof(IIdentity));
            Assert.IsNotNull(identity);
            var rec = identity as TypeDescriptor.Record;
            Assert.IsNotNull(rec);
            Assert.AreEqual(1, rec.Properties.Count);
            var prop = rec.Properties.First().Value;
            Assert.AreEqual("Href", prop.Name);
            Assert.IsTrue(prop.Required);
            Assert.IsInstanceOfType(prop.Type, typeof(TypeDescriptor.Primitive.String));
        }
        [TestMethod]
        public void TypeDescriptor_RestResult()
        {
            var tdc = new StandardTypeDescriptorCreator(new ITypeRepresentation[] {
                RestResultTypeRepresentation.Instance,
                RestValueTypeRepresentation.Instance,
                new RestJobRepresentation(),
                new RestJobResultRepresentation(),
                TestIdentityRepresentation.Instance
            });
            var desc = tdc.GetDescriptor(typeof(RestResult<Person>));
            if (desc is TypeDescriptor.Union u)
            {
                Assert.AreEqual(8, u.Options.Count);
                foreach (var tag in new[] { "Success", "Error", "NotFound", "BadRequest", "Temporary", "Permanent", "NotExecuted", "NotNecessary" })
                    Assert.IsTrue(u.Options.Any(o => o is TypeDescriptor.Record r && r.Properties.ContainsKey(tag)));
                var props = u.Options
                    .OfType<TypeDescriptor.Record>()
                    .Where(r => r.Properties.ContainsKey("Success"))
                    .SelectMany(r => ((TypeDescriptor.Record)r.Properties["Success"].Type).Properties.Values)
                    .Where(p => p.Name == nameof(IRestValue<object>.Value))
                    .Select(p => ((TypeDescriptor.Record)((TypeDescriptor.Referable)p.Type).ExpandedDescriptor).Properties)
                    .First();
                Assert.IsTrue(props.ContainsKey(nameof(Person.FirstName)));
                Assert.IsTrue(props.ContainsKey(nameof(Person.LastName)));
                Assert.IsTrue(props.ContainsKey(nameof(Person.Age)));
            }
            else
                Assert.Fail();
        }

        public struct EmailAddress
        {
            public EmailAddress(string address)
            {
                Address = address;
            }
            public string Address { get; }
        }
    }
}
