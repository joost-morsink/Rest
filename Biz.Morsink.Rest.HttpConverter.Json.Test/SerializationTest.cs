using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    [TestClass]
    public class SerializationTest
    {
        private TestRestRequestScopeAccessor restRequestScopeAccessor;
        private TestOptions options;
        private RestJsonContractResolver resolver;
        private JsonSerializer serializer;

        [TestInitialize]
        public void Init()
        {
            restRequestScopeAccessor = new TestRestRequestScopeAccessor();
            options = new TestOptions();
            resolver = new RestJsonContractResolver(Enumerable.Empty<IJsonSchemaTranslator>(), Enumerable.Empty<ITypeRepresentation>(), restRequestScopeAccessor, options);
            serializer = new JsonSerializer();
            serializer.ContractResolver = resolver;

        }
        [TestMethod]
        public void JsonSerialize_Normal()
        {
            var o = new A { B = 1, C = 2, D = "abc" };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<A>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_NormalImm()
        {
            var o = new ImmA(1, 2, "abc");
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<ImmA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_NormalMixed()
        {
            var o = new MixedA(1, 2) { D = "abc" };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<MixedA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingStruct()
        {
            var o = new A { B = 1, D = "abc" };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<A>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingStructImm()
        {
            var o = new ImmA(1, null, "abc");
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<ImmA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingStructMixed()
        {
            var o = new MixedA(1, null) { D = "abc" };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["D"]);

            var back = json.ToObject<MixedA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingClass()
        {
            var o = new A { B = 1, C = 2 };
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);

            var back = json.ToObject<A>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingClassImm()
        {
            var o = new ImmA(1, 2, null);
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);

            var back = json.ToObject<ImmA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        [TestMethod]
        public void JsonSerialize_MissingClassMixed()
        {
            var o = new MixedA(1, 2);
            var json = JObject.FromObject(o, serializer);
            var text = json?.ToString();
            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Properties().Count());
            Assert.IsNotNull(json["B"]);
            Assert.IsNotNull(json["C"]);

            var back = json.ToObject<MixedA>(serializer);
            Assert.IsTrue(back.Equals(o));
        }
        
        // Would like to have this work, but does not work in original Json.Net either...
        //[TestMethod]
        //public void JsonSerialize_DefaultCtorParam()
        //{
        //    var o = new DefCtorParamA(1, 2);

        //    var json = JObject.Parse("{B:1, C:2}");
        //    var back = json.ToObject<DefCtorParamA>(serializer);
        //    Assert.IsTrue(back.Equals(o));
        //    Assert.AreEqual("xyz", back.D);
        //}
        public class A : IEquatable<A>
        {
            public int B { get; set; }
            public int? C { get; set; }
            public string D { get; set; }
            public override int GetHashCode()
                => B;
            public override bool Equals(object obj)
                => obj is A a ? Equals(a) : false;
            public bool Equals(A other)
                => B == other.B && C == other.C && D == other.D;
            public static bool operator ==(A a, A b)
                => a.Equals(b);
            public static bool operator !=(A a, A b)
                => !a.Equals(b);
        }
        public class ImmA : IEquatable<ImmA>
        {
            public ImmA(int b, int? c, string d)
            {
                B = b;
                C = c;
                D = d;
            }
            public int B { get; }
            public int? C { get; }
            public string D { get; }
            public override int GetHashCode()
                => B;
            public override bool Equals(object obj)
                => obj is ImmA a ? Equals(a) : false;
            public bool Equals(ImmA other)
                => B == other.B && C == other.C && D == other.D;
            public static bool operator ==(ImmA a, ImmA b)
                => a.Equals(b);
            public static bool operator !=(ImmA a, ImmA b)
                => !a.Equals(b);
        }
        public class MixedA : IEquatable<MixedA>
        {
            public MixedA(int b, int? c)
            {
                B = b;
                C = c;
            }
            public int B { get; set; }
            public int? C { get; set; }
            public string D { get; set; }
            public override int GetHashCode()
                => B;
            public override bool Equals(object obj)
                => obj is MixedA a ? Equals(a) : false;
            public bool Equals(MixedA other)
                => B == other.B && C == other.C && D == other.D;
            public static bool operator ==(MixedA a, MixedA b)
                => a.Equals(b);
            public static bool operator !=(MixedA a, MixedA b)
                => !a.Equals(b);
        }
        //public class DefCtorParamA : IEquatable<DefCtorParamA>
        //{
        //    public DefCtorParamA(int b, int? c, string d = "xyz")
        //    {
        //        B = b;
        //        C = c;
        //        D = d;
        //    }
        //    public int B { get; }
        //    public int? C { get; }
        //    public string D { get; }
        //    public override int GetHashCode()
        //        => B;
        //    public override bool Equals(object obj)
        //        => obj is DefCtorParamA a ? Equals(a) : false;
        //    public bool Equals(DefCtorParamA other)
        //        => B == other.B && C == other.C && D == other.D;
        //    public static bool operator ==(DefCtorParamA a, DefCtorParamA b)
        //        => a.Equals(b);
        //    public static bool operator !=(DefCtorParamA a, DefCtorParamA b)
        //        => !a.Equals(b);
        //}
    }
    public class TestRestRequestScopeAccessor : IRestRequestScopeAccessor
    {
        public IRestRequestScope Scope => throw new NotImplementedException();

        private class RestRequestScope : IRestRequestScope
        {
            private TypeKeyedDictionary dict;

            public RestRequestScope()
            {
                dict = TypeKeyedDictionary.Empty;
            }
            public void SetScopeItem<T>(T item)
            {
                dict = dict.Set(item);
            }

            public bool TryGetScopeItem<T>(out T result)
                => dict.TryGet(out result);
            public bool TryRemoveScopeItem<T>(out T result)
            {
                if (TryGetScopeItem(out result))
                {
                    dict = dict.Set(default(T));
                    return true;
                }
                else
                    return false;
            }
        }
    }
    public class TestOptions : IOptions<JsonHttpConverterOptions>
    {
        public JsonHttpConverterOptions Value => new JsonHttpConverterOptions().ApplyDefaultNamingStrategy().UseEmbeddings();
    }
}
