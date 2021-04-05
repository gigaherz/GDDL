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
        public void TestIntegersCompact()
        {
            Assert.AreEqual("0", FormatOneCompact(Value.Of(0)));
            Assert.AreEqual("1", FormatOneCompact(Value.Of(1)));
            Assert.AreEqual("10", FormatOneCompact(Value.Of(10)));
            Assert.AreEqual("100", FormatOneCompact(Value.Of(100)));
            Assert.AreEqual("1000000000000000000", FormatOneCompact(Value.Of(1000000000000000000L)));
            Assert.AreEqual("9223372036854775807", FormatOneCompact(Value.Of(long.MaxValue)));
            Assert.AreEqual("-1", FormatOneCompact(Value.Of(-1)));
            Assert.AreEqual("-10", FormatOneCompact(Value.Of(-10)));
            Assert.AreEqual("-100", FormatOneCompact(Value.Of(-100)));
            Assert.AreEqual("-1000000000000000000", FormatOneCompact(Value.Of(-1000000000000000000L)));
            Assert.AreEqual("-9223372036854775808", FormatOneCompact(Value.Of(long.MinValue)));
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
            Assert.AreEqual("1.0e18", FormatOneCompact(Value.Of(1000000000000000000.0)));
            Assert.AreEqual("1.999999999999999e15", FormatOneCompact(Value.Of(1999999999999999.0)));
            Assert.AreEqual("2.0e32", FormatOneCompact(Value.Of(199999999999999999999999999999999.0)));
            Assert.AreEqual(".NaN", FormatOneCompact(Value.Of(float.NaN)));
            Assert.AreEqual(".Inf", FormatOneCompact(Value.Of(float.PositiveInfinity)));
            Assert.AreEqual("-.Inf", FormatOneCompact(Value.Of(float.NegativeInfinity)));
        }

        [TestMethod]
        public void TestStringsCompact()
        {
            Assert.AreEqual("\"1\"", FormatOneCompact(Value.Of("1")));
            Assert.AreEqual("\"\\x03\"", FormatOneCompact(Value.Of("\x03")));
            Assert.AreEqual("\"\\uFFFA\"", FormatOneCompact(Value.Of("\uFFFA")));
        }

        public static string FormatOneCompact(Element e)
        {
            var b = new StringBuilder();
            new Formatter(b, FormatterOptions.Compact).FormatStandalone(e);
            return b.ToString();
        }
    }
}
