using Biz.Morsink.RestServer.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Test
{
    [TestClass]
    public class RestPathTest
    {
        [TestMethod]
        public void RestPath_Trivial()
        {
            var p = RestPath.Parse("/api/person/1", null);
            Assert.AreEqual(4, p.Count, "Count property should count all parts.");
            Assert.IsTrue(p[0].Content == "" && p[1].Content == "api" && p[2].Content == "person" && p[3].Content == "1", "The parsed RestPath should match the parts in number and order of the parts in the original RestPath string.");
        }
        [TestMethod]
        public void RestPath_NoStar()
        {
            var p = RestPath.Parse("/api/home");
            var q = RestPath.Parse("/api/home");
            var m = p.MatchPath(q);
            Assert.IsTrue(m.IsSuccessful, "Two equal RestPaths should match.");
            Assert.AreEqual(0, m.SegmentValues.Count, "No wildcards should result in a 0-ary match.");
        }
        [TestMethod]
        public void RestPath_Star()
        {
            var p = RestPath.Parse("/api/person/*/test", null);
            var q = RestPath.Parse("/api/person/123/test", null);
            var m = p.MatchPath(q);
            Assert.IsTrue(m.IsSuccessful, "A wildcard should match any value.");
            Assert.AreEqual(1, m.SegmentValues.Count, "One wildcard should result in a unary match.");
            Assert.AreEqual("123", m[0], "The matched wildcard part should match the one in the RestPath string.");
        }
        [TestMethod]
        public void RestPath_DoubleStar()
        {
            var p = RestPath.Parse("/api/person/*/detail/*");
            var q = RestPath.Parse("/api/person/123/detail/456");
            var m = p.MatchPath(q);
            Assert.IsTrue(m.IsSuccessful, "Two wildcards should each match any value.");
            Assert.AreEqual(2, m.SegmentValues.Count, "Two wildcards should result in a binary match.");
            Assert.IsTrue(m[0] == "123" && m[1] == "456", "The matched wildcard parts should match those in the RestPath string in the same order.");
        }

    }
}
