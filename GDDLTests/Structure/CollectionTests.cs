﻿using GDDL.Structure;
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
            Collection collection = Collection.Of(Value.Of(1));
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void CollectionOfAddsNames()
        {
            var named = Value.Of(true).WithName("test");
            Collection collection = Collection.Of(named);
            List<Element> l = collection.ByName("test").ToList();
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(l[0], named);
        }

        [TestMethod]
        public void CollectionAddAddsElements()
        {
            Collection collection = Collection.Empty();
            Assert.AreEqual(0, collection.Count);
            collection.Add(Value.Of(1));
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void CollectionAddAddsNames()
        {
            var named = Value.Of(1).WithName("test");
            Collection collection = Collection.Empty();
            Assert.AreEqual(0, collection.Count);
            collection.Add(named);
            List<Element> l = collection.ByName("test").ToList();
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(l[0], named);
        }

        [TestMethod]
        public void CollectionAddAllAddsElements()
        {
            Collection collection = Collection.Empty();
            Assert.AreEqual(0, collection.Count);
            collection.AddRange(new Element[] { Value.Of(1), Value.Of(2), Value.Of(3) });
            Assert.AreEqual(3, collection.Count);
        }

        [TestMethod]
        public void CollectionGetReturnsElements()
        {
            Value second = Value.Of(2);
            Collection collection = Collection.Of(Value.Of(1), second, Value.Of(3));
            Assert.AreEqual(second, collection[1]);
        }

        [TestMethod]
        public void CollectionInsertInsertsElements()
        {
            Value second = Value.Of(2);
            Value third = Value.Of(3);
            Collection collection = Collection.Of(Value.Of(1), second, Value.Of(4));
            Assert.AreEqual(second, collection[1]);
            collection.Insert(1, third);
            Assert.AreEqual(third, collection[1]);
        }
    }

}
