using GDDL.Queries;
using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GDDL.Tests.Queries
{
    [TestClass]
    public class QueryTests
    {
        [TestMethod]
        public void QueryObjectReturnsElement()
        {
            var map = GddlMap.Of(
                   new("key1", GddlValue.Of("Text")),
                   new("key2", GddlValue.Of(1))
            );
            AssertListsEqual([GddlValue.Of("Text")], Query.FromString("/key1").Apply(map).ToList());
            AssertListsEqual([GddlValue.Of(1)], Query.FromString("/key2").Apply(map).ToList());
            AssertListsEqual([], Query.FromString("/key3").Apply(map).ToList());
        }

        [TestMethod]
        public void QueryListReturnsElement()
        {
            var list = GddlList.Of(
                    GddlValue.Of("Text"),
                    GddlValue.Of(1)
            );
            AssertListsEqual([GddlValue.Of("Text")], Query.FromString("/[0]").Apply(list).ToList());
            AssertListsEqual([GddlValue.Of(1)], Query.FromString("/[1]").Apply(list).ToList());
            AssertListsEqual([], Query.FromString("/[2]").Apply(list).ToList());
            AssertListsEqual([GddlValue.Of(1)], Query.FromString("/[^1]").Apply(list).ToList());
            AssertListsEqual([GddlValue.Of("Text")], Query.FromString("/[^2]").Apply(list).ToList());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Query.FromString("/[-1]").Apply(list).ToList());
        }

        [TestMethod]
        public void QueryListRangeReturnsRange()
        {
            var list = GddlList.Of(
                    GddlValue.Of("Text"),
                    GddlValue.Of(1),
                    GddlValue.Of(false)
            );
            AssertListsEqual([GddlValue.Of("Text")], Query.FromString("/[0..1]").Apply(list).ToList());
            AssertListsEqual([GddlValue.Of("Text"), GddlValue.Of(1)], Query.FromString("/[0...1]").Apply(list).ToList());
            AssertListsEqual([GddlValue.Of("Text")], Query.FromString("/[..1]").Apply(list).ToList());
            AssertListsEqual([GddlValue.Of("Text"), GddlValue.Of(1)], Query.FromString("/[..^1]").Apply(list).ToList());
            AssertListsEqual([GddlValue.Of("Text"), GddlValue.Of(1), GddlValue.Of(false)], Query.FromString("/[0..^0]").Apply(list).ToList());
            AssertListsEqual([GddlValue.Of(1), GddlValue.Of(false)], Query.FromString("/[^2..^0]").Apply(list).ToList());
        }

        [TestMethod]
        public void QueryListInsideObject()
        {
            var list2 = GddlList.Of(
                    GddlValue.Of(12345)
            );
            var list = GddlList.Of(
                    GddlValue.Of("A"),
                    GddlValue.Of(314),
                    list2
            );
            var map = GddlMap.Of(
                    new("key1", GddlValue.Of("Text")),
                    new("key2", GddlValue.Of(1)),
                    new("key3", list)
            );
            AssertListsEqual([GddlValue.Of(314)], Query.FromString("/key3/[1..^1]").Apply(map).ToList());
            AssertListsEqual([GddlValue.Of(314)], Query.FromString("/key3[1..^1]").Apply(map).ToList());
            AssertListsEqual([GddlValue.Of(12345)], Query.FromString("/key3[2][0]").Apply(map).ToList());
        }



        [TestMethod]
        public void QueryObjectInsideList()
        {
            var map1 = new GddlMap() {
                   { "key1", GddlValue.Of("Text")},
                   { "key2", GddlValue.Of(1) }
            };
            var list = new GddlList() {
                    GddlValue.Of("A"),
                    GddlValue.Of(314),
                    map1
            };
            AssertListsEqual([GddlValue.Of("Text")], Query.FromString("/[2]/key1").Apply(list).ToList());
            AssertListsEqual([GddlValue.Of(1)], Query.FromString("/[2]/key2").Apply(list).ToList());
        }

        private static void AssertListsEqual<T>(List<T> expected, List<T> actual)
        {
            if (expected.Count != actual.Count)
                Assert.Fail($"Lists not the same length. Expected:<{expected.Count}>. Actual:<{actual.Count}>");
            for (int i = 0; i < expected.Count; i++)
            {
                if (!Equals(expected[i], actual[i]))
                    Assert.Fail($"Element {i} not equal. Expected:<{expected[i]}>. Actual:<{actual[i]}>");
            }
        }
    }
}
