using GDDL.Queries;
using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDDL.Tests.Queries
{
    [TestClass]
    public class QueryTests
    {
        [TestMethod]
        public void QueryObjectReturnsElement()
            {
            var map = GddlMap.Of(
                   new ("key1", GddlValue.Of("Text")),
                   new ( "key2", GddlValue.Of(1) )
            );
            AssertListsEqual(Of(GddlValue.Of("Text")), Query.FromString("/key1").Apply(map).ToList());
            AssertListsEqual(Of(GddlValue.Of(1)), Query.FromString("/key2").Apply(map).ToList());
            AssertListsEqual(Of(), Query.FromString("/key3").Apply(map).ToList());
        }

        [TestMethod]
        public void QueryListReturnsElement()
        {
            var list = GddlList.Of(
                    GddlValue.Of("Text"),
                    GddlValue.Of(1)
            );
            AssertListsEqual(Of(GddlValue.Of("Text")), Query.FromString("/[0]").Apply(list).ToList());
            AssertListsEqual(Of(GddlValue.Of(1)), Query.FromString("/[1]").Apply(list).ToList());
            AssertListsEqual(Of(), Query.FromString("/[2]").Apply(list).ToList());
            AssertListsEqual(Of(GddlValue.Of(1)), Query.FromString("/[^1]").Apply(list).ToList());
            AssertListsEqual(Of(GddlValue.Of("Text")), Query.FromString("/[^2]").Apply(list).ToList());
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
            AssertListsEqual(Of(GddlValue.Of("Text")), Query.FromString("/[0..1]").Apply(list).ToList());
            AssertListsEqual(Of(GddlValue.Of("Text"), GddlValue.Of(1)), Query.FromString("/[0...1]").Apply(list).ToList());
            AssertListsEqual(Of(GddlValue.Of("Text")), Query.FromString("/[..1]").Apply(list).ToList());
            AssertListsEqual(Of(GddlValue.Of("Text"), GddlValue.Of(1)), Query.FromString("/[..^1]").Apply(list).ToList());
            AssertListsEqual(Of(GddlValue.Of("Text"), GddlValue.Of(1), GddlValue.Of(false)), Query.FromString("/[0..^0]").Apply(list).ToList());
            AssertListsEqual(Of(GddlValue.Of(1), GddlValue.Of(false)), Query.FromString("/[^2..^0]").Apply(list).ToList());
        }

        private static List<GddlElement> Of(params GddlElement[] elements)
        {
            return elements.ToList();
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
