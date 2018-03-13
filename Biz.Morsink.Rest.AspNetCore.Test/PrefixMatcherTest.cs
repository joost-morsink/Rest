using Biz.Morsink.Rest.AspNetCore.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Test
{
    [TestClass]
    public class PrefixMatcherTest
    {
        [TestMethod]
        public void PrefixMatch_Test()
        {
            var m = PrefixMatcher<int>.Empty
                .Add("abcdefgh", 1)
                .Add("abcdijkl", 2)
                .Add("ab123", 3)
                .Add("ab12", 4)
                .Add("ab1234",5)
                .Add("abcdijkm", 7)
                .Add("abcdijn", 8)
                ;

            Assert.IsTrue(m.TryMatch("abcdefgh", out var x));
            Assert.AreEqual(1, x);
            Assert.IsTrue(m.TryMatch("abcdijklmnop", out x));
            Assert.AreEqual(2, x);
            Assert.IsTrue(m.TryMatch("abcdijkmnop", out x));
            Assert.AreEqual(7, x);
            Assert.IsTrue(m.TryMatch("abcdijnop", out x));
            Assert.AreEqual(8, x);
            Assert.IsFalse(m.TryMatch("abc123", out x));
            Assert.IsTrue(m.TryMatch("ab12356", out x));
            Assert.AreEqual(3, x);
            Assert.IsTrue(m.TryMatch("ab12", out x));
            Assert.AreEqual(4, x);
            Assert.IsTrue(m.TryMatch("ab123", out x));
            Assert.AreEqual(3, x);
            Assert.IsTrue(m.TryMatch("ab124", out x));
            Assert.AreEqual(4, x);
            Assert.IsTrue(m.TryMatch("ab1234", out x));
            Assert.AreEqual(5, x);
            Assert.IsTrue(m.TryMatch("ab12345", out x));
            Assert.AreEqual(5, x);
            Assert.IsFalse(m.TryMatch("ac", out x));
            Assert.IsFalse(m.TryMatch("abcdefkl", out x));
            Assert.IsFalse(m.TryMatch("bcde", out x));
            Assert.IsFalse(m.TryMatch("b", out x));
            Assert.IsFalse(m.TryMatch("", out x));


            m = m.Add("a", 6);
            Assert.IsTrue(m.TryMatch("abcdefgh", out x));
            Assert.AreEqual(1, x);
            Assert.IsTrue(m.TryMatch("abcdijklmnop", out x));
            Assert.AreEqual(2, x);
            Assert.IsTrue(m.TryMatch("abcdijkmnop", out x));
            Assert.AreEqual(7, x);
            Assert.IsTrue(m.TryMatch("abcdijnop", out x));
            Assert.AreEqual(8, x);
            Assert.IsTrue(m.TryMatch("abc123", out x));
            Assert.AreEqual(6, x);
            Assert.IsTrue(m.TryMatch("ab12356", out x));
            Assert.AreEqual(3, x);
            Assert.IsTrue(m.TryMatch("ab12", out x));
            Assert.AreEqual(4, x);
            Assert.IsTrue(m.TryMatch("ab123", out x));
            Assert.AreEqual(3, x);
            Assert.IsTrue(m.TryMatch("ab124", out x));
            Assert.AreEqual(4, x);
            Assert.IsTrue(m.TryMatch("ab1234", out x));
            Assert.AreEqual(5, x);
            Assert.IsTrue(m.TryMatch("ab12345", out x));
            Assert.AreEqual(5, x);
            Assert.IsTrue(m.TryMatch("ac", out x));
            Assert.AreEqual(6, x);
            Assert.IsTrue(m.TryMatch("abcdefkl", out x));
            Assert.AreEqual(6, x);
            Assert.IsFalse(m.TryMatch("bcde", out x));
            Assert.IsFalse(m.TryMatch("b", out x));
            Assert.IsFalse(m.TryMatch("", out x));

        }
    }
}
