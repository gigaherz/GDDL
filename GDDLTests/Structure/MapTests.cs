using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GDDL.Tests.Structure
{
    [TestClass]
    public class MapTests
    {
        [TestMethod]
        public void EmptyMapContainsNoItems()
        {
            var collection = GddlMap.Empty();
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void MapOfAddsNames()
        {
            var element = GddlValue.Of(true);
            var collection = new GddlMap() { { "test", element } };
            var value = collection["test"];
            Assert.IsNotNull(value);
            Assert.AreEqual(element, value);
        }
    }
}