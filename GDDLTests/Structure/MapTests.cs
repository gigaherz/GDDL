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
            GddlMap collection = GddlMap.Empty();
            Assert.AreEqual(0, collection.Count);
        }
    }
}