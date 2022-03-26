using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GDDL.Tests.Structure
{
    [TestClass]
    public class ListTests
    {
        [TestMethod]
        public void EmptyListContainsNoItems()
        {
            GddlList collection = GddlList.Empty();
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void ListOfAddsElements()
        {
            GddlList collection = GddlList.Of(GddlValue.Of(1));
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void ListAddAddsElements()
        {
            GddlList collection = GddlList.Empty();
            Assert.AreEqual(0, collection.Count);
            collection.Add(GddlValue.Of(1));
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void ListAddAllAddsElements()
        {
            GddlList collection = GddlList.Empty();
            Assert.AreEqual(0, collection.Count);
            collection.AddRange(new GddlElement[] { GddlValue.Of(1), GddlValue.Of(2), GddlValue.Of(3) });
            Assert.AreEqual(3, collection.Count);
        }

        [TestMethod]
        public void ListGetReturnsElements()
        {
            GddlValue second = GddlValue.Of(2);
            GddlList collection = GddlList.Of(GddlValue.Of(1), second, GddlValue.Of(3));
            Assert.AreEqual(second, collection[1]);
        }

        [TestMethod]
        public void ListInsertInsertsElements()
        {
            GddlValue second = GddlValue.Of(2);
            GddlValue third = GddlValue.Of(3);
            GddlList collection = GddlList.Of(GddlValue.Of(1), second, GddlValue.Of(4));
            Assert.AreEqual(second, collection[1]);
            collection.Insert(1, third);
            Assert.AreEqual(third, collection[1]);
        }

        [TestMethod]
        public void ListRemoveElementRemovesElements()
        {
            GddlValue first = GddlValue.Of(1);
            GddlValue second = GddlValue.Of("test");
            GddlList collection = GddlList.Of(first, second);
            Assert.AreEqual(2, collection.Count);
            collection.Remove(second);
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(first, collection[0]);
        }

        [TestMethod]
        public void ListRemoveIndexRemovesElements()
        {
            GddlValue second = GddlValue.Of("test");
            GddlList collection = GddlList.Of(GddlValue.Of(1), second);
            Assert.AreEqual(2, collection.Count);
            collection.RemoveAt(0);
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(second, collection[0]);
        }
    }
}