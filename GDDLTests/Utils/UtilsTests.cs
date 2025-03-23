using GDDL.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
