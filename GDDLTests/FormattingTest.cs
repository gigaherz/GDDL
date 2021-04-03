using GDDL.Serialization;
using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace GDDL.Tests
{
    [TestClass]
    public class FormattingTest
    {
        [TestMethod]
        public void TestValuesCompact()
        {
            Assert.AreEqual("null", FormatOneCompact(Value.Null()));
            Assert.AreEqual("false", FormatOneCompact(Value.Of(false)));
            Assert.AreEqual("true", FormatOneCompact(Value.Of(true)));
            Assert.AreEqual("1", FormatOneCompact(Value.Of(1)));
            Assert.AreEqual("1.0", FormatOneCompact(Value.Of(1.0)));
            Assert.AreEqual("\"1\"", FormatOneCompact(Value.Of("1")));
        }

        [TestMethod]
        public void TestFloatsCompact()
        {
            Assert.AreEqual("1.0", FormatOneCompact(Value.Of(1.0)));
            Assert.AreEqual("1.01", FormatOneCompact(Value.Of(1.01)));
            Assert.AreEqual("1.000000000001", FormatOneCompact(Value.Of(1.000000000001)));
            Assert.AreEqual("0.1", FormatOneCompact(Value.Of(0.1)));
            Assert.AreEqual("1.0e-8", FormatOneCompact(Value.Of(0.00000001)));
            Assert.AreEqual("1.0e10", FormatOneCompact(Value.Of(10000000000.0)));
            Assert.AreEqual("3.0e-50", FormatOneCompact(Value.Of(3e-50)));
            Assert.AreEqual("1.999999999999999e15", FormatOneCompact(Value.Of(1999999999999999.0)));
            Assert.AreEqual("2.0e32", FormatOneCompact(Value.Of(199999999999999999999999999999999.0)));
        }

        [TestMethod]
        public void TestStringsCompact()
        {
            Assert.AreEqual("\"1\"", FormatOneCompact(Value.Of("1")));
        }

        public static String FormatOneCompact(Element e)
        {
            StringBuilder b = new StringBuilder();
            new Formatter(b, FormatterOptions.Compact).FormatStandalone(e);
            return b.ToString();
        }
    }
}
