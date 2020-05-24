using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDDL.Tests
{
    [TestClass()]
    public class StructureTests
    {
        [TestMethod()]
        public void collectionOfAddsNames()
        {
            Element value = Value.Of(true).WithName("test");
            Collection collection = Collection.Of(value);
            List<Element> l = collection.ByName("test").ToList();
            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(l[0], value);
        }
    }

}
