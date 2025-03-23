using GDDL.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GDDL.Tests.Utils
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void UnescapeWorks()
        {
            Assert.AreEqual("\r", Utility.UnescapeString("'\\\r'"));
            Assert.AreEqual("\n", Utility.UnescapeString("'\\\n'"));
            Assert.AreEqual("\r\n", Utility.UnescapeString("'\\\r\\\n'"));
            Assert.AreEqual("\r\n", Utility.UnescapeString("'\\\r\n'"));
            Assert.AreEqual("\r\n", Utility.UnescapeString("'\r\\\n'"));
        }
    }
}
