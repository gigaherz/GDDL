using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDDL.Tests.Structure
{
    [TestClass]
    public class CollectionTests
    {
        [TestMethod]
        public void EmptyCollectionContainsNoItems()
        {
            Collection collection = Collection.Empty();
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void CollectionOfAddsElements()
        {
            Collection collection = Collection.Of(GddlValue.Of(1));
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void CollectionOfAddsNames()
        {
            var named = GddlValue.Of(true).WithName("test");
            Collection collection = Collection.Of(named);
            List<GddlElement> l = collection.ByName("test").ToList();
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(l[0], named);
        }

        [TestMethod]
        public void CollectionAddAddsElements()
        {
            Collection collection = Collection.Empty();
            Assert.AreEqual(0, collection.Count);
            collection.Add(GddlValue.Of(1));
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void CollectionAddAddsNames()
        {
            var named = GddlValue.Of(1).WithName("test");
            Collection collection = Collection.Empty();
            Assert.AreEqual(0, collection.Count);
            collection.Add(named);
            List<GddlElement> l = collection.ByName("test").ToList();
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(l[0], named);
        }

        [TestMethod]
        public void CollectionAddAllAddsElements()
        {
            Collection collection = Collection.Empty();
            Assert.AreEqual(0, collection.Count);
            collection.AddRange(new GddlElement[] { GddlValue.Of(1), GddlValue.Of(2), GddlValue.Of(3) });
            Assert.AreEqual(3, collection.Count);
        }

        [TestMethod]
        public void CollectionGetReturnsElements()
        {
            GddlValue second = GddlValue.Of(2);
            Collection collection = Collection.Of(GddlValue.Of(1), second, GddlValue.Of(3));
            Assert.AreEqual(second, collection[1]);
        }

        [TestMethod]
        public void CollectionInsertInsertsElements()
        {
            GddlValue second = GddlValue.Of(2);
            GddlValue third = GddlValue.Of(3);
            Collection collection = Collection.Of(GddlValue.Of(1), second, GddlValue.Of(4));
            Assert.AreEqual(second, collection[1]);
            collection.Insert(1, third);
            Assert.AreEqual(third, collection[1]);
        }

        [TestMethod]
        public void CollectionRemoveElementRemovesElements()
        {
            GddlValue first = GddlValue.Of(1);
            GddlValue second = GddlValue.Of("test");
            Collection collection = Collection.Of(first, second);
            Assert.AreEqual(2, collection.Count);
            collection.Remove(second);
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(first, collection[0]);
        }

        [TestMethod]
        public void CollectionRemoveIndexRemovesElements()
        {
            GddlValue second = GddlValue.Of("test");
            Collection collection = Collection.Of(GddlValue.Of(1), second);
            Assert.AreEqual(2, collection.Count);
            collection.RemoveAt(0);
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(second, collection[0]);
        }

        [TestMethod]
        public void CollectionRemoveElementRemovesNames()
        {
            GddlValue first = GddlValue.Of(1);
            GddlValue second = GddlValue.Of("test");
            Collection collection = Collection.Of(first, second);
            Assert.AreEqual(2, collection.Count);
            collection.Remove(second);
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(first, collection[0]);
            List<GddlElement> byName = collection.ByName("test").ToList();
            Assert.AreEqual(0, byName.Count);
        }

        [TestMethod]
        public void CollectionRemoveIndexRemovesNames()
        {
            GddlValue named = GddlValue.Of("test").WithName("test");
            Collection collection = Collection.Of(named, GddlValue.Of(1));
            Assert.AreEqual(2, collection.Count);
            collection.RemoveAt(0);
            Assert.AreEqual(1, collection.Count);
            List<GddlElement> byName = collection.ByName("test").ToList();
            Assert.AreEqual(0, byName.Count);
        }
    }
}
