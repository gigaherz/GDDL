using GDDL.Serialization;
using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace GDDL.Tests
{
    [TestClass]
    public class FormattingTest
    {
        [TestMethod]
        public void TestValuesCompact()
        {
            Assert.AreEqual("null", FormatOneCompact(GddlValue.Null()));
            Assert.AreEqual("false", FormatOneCompact(GddlValue.Of(false)));
            Assert.AreEqual("true", FormatOneCompact(GddlValue.Of(true)));
            Assert.AreEqual("1", FormatOneCompact(GddlValue.Of(1)));
            Assert.AreEqual("1.0", FormatOneCompact(GddlValue.Of(1.0)));
            Assert.AreEqual("\"1\"", FormatOneCompact(GddlValue.Of("1")));
        }

        [TestMethod]
        public void TestIntegersCompact()
        {
            Assert.AreEqual("0", FormatOneCompact(GddlValue.Of(0)));
            Assert.AreEqual("1", FormatOneCompact(GddlValue.Of(1)));
            Assert.AreEqual("10", FormatOneCompact(GddlValue.Of(10)));
            Assert.AreEqual("100", FormatOneCompact(GddlValue.Of(100)));
            Assert.AreEqual("1000000000000000000", FormatOneCompact(GddlValue.Of(1000000000000000000L)));
            Assert.AreEqual("9223372036854775807", FormatOneCompact(GddlValue.Of(long.MaxValue)));
            Assert.AreEqual("-1", FormatOneCompact(GddlValue.Of(-1)));
            Assert.AreEqual("-10", FormatOneCompact(GddlValue.Of(-10)));
            Assert.AreEqual("-100", FormatOneCompact(GddlValue.Of(-100)));
            Assert.AreEqual("-1000000000000000000", FormatOneCompact(GddlValue.Of(-1000000000000000000L)));
            Assert.AreEqual("-9223372036854775808", FormatOneCompact(GddlValue.Of(long.MinValue)));
        }

        [TestMethod]
        public void TestFloatsCompact()
        {
            Assert.AreEqual("1.0", FormatOneCompact(GddlValue.Of(1.0)));
            Assert.AreEqual("1.01", FormatOneCompact(GddlValue.Of(1.01)));
            Assert.AreEqual("1.000000000001", FormatOneCompact(GddlValue.Of(1.000000000001)));
            Assert.AreEqual("0.1", FormatOneCompact(GddlValue.Of(0.1)));
            Assert.AreEqual("1.0e-8", FormatOneCompact(GddlValue.Of(0.00000001)));
            Assert.AreEqual("1.0e10", FormatOneCompact(GddlValue.Of(10000000000.0)));
            Assert.AreEqual("3.0e-50", FormatOneCompact(GddlValue.Of(3e-50)));
            Assert.AreEqual("1.0e18", FormatOneCompact(GddlValue.Of(1000000000000000000.0)));
            Assert.AreEqual("1.99999999999999e14", FormatOneCompact(GddlValue.Of(199999999999999.0)));
            Assert.AreEqual("2.0e15", FormatOneCompact(GddlValue.Of(1999999999999999.0)));
            Assert.AreEqual("2.0e32", FormatOneCompact(GddlValue.Of(199999999999999999999999999999999.0)));
            Assert.AreEqual(".NaN", FormatOneCompact(GddlValue.Of(float.NaN)));
            Assert.AreEqual(".Inf", FormatOneCompact(GddlValue.Of(float.PositiveInfinity)));
            Assert.AreEqual("-.Inf", FormatOneCompact(GddlValue.Of(float.NegativeInfinity)));
        }

        [TestMethod]
        public void TestStringsCompact()
        {
            Assert.AreEqual("\"1\"", FormatOneCompact(GddlValue.Of("1")));
            Assert.AreEqual("\"\\x03\"", FormatOneCompact(GddlValue.Of("\x03")));
            Assert.AreEqual("\"\\uFFFA\"", FormatOneCompact(GddlValue.Of("\uFFFA")));
        }

        [TestMethod]
        public void TestListsCompact()
        {
            Assert.AreEqual("[]", FormatOneCompact(GddlList.Empty()));
            Assert.AreEqual("[[]]", FormatOneCompact(GddlList.Of(GddlList.Empty())));
            Assert.AreEqual("[[],[]]", FormatOneCompact(GddlList.Of(GddlList.Empty(), GddlList.Empty())));
            Assert.AreEqual("[[],{}]", FormatOneCompact(GddlList.Of(GddlList.Empty(), GddlMap.Empty())));
            Assert.AreEqual("[1]", FormatOneCompact(GddlList.Of(GddlValue.Of(1))));
            Assert.AreEqual("[\"1\"]", FormatOneCompact(GddlList.Of(GddlValue.Of("1"))));
            Assert.AreEqual("[null]", FormatOneCompact(GddlList.Of(GddlValue.Null())));
        }

        [TestMethod]
        public void TestMapsCompact()
        {
            Assert.AreEqual("{}", FormatOneCompact(GddlMap.Empty()));
            Assert.AreEqual("{a=[]}", FormatOneCompact(new GddlMap() { { "a", GddlList.Empty() } }));
            Assert.AreEqual("{a=[],b=[]}", FormatOneCompact(new GddlMap() { { "a", GddlList.Empty() }, { "b", GddlList.Empty() } }));
            Assert.AreEqual("{a=[],b={}}", FormatOneCompact(new GddlMap() { { "a", GddlList.Empty() }, { "b", GddlMap.Empty() } }));
            Assert.AreEqual("{a=1}", FormatOneCompact(new GddlMap() { { "a", GddlValue.Of(1) } }));
            Assert.AreEqual("{a=\"1\"}", FormatOneCompact(new GddlMap() { { "a", GddlValue.Of("1") } }));
            Assert.AreEqual("{a=null}", FormatOneCompact(new GddlMap() { { "a", GddlValue.Null() } }));
            Assert.AreEqual("{\"a b\"=1}", FormatOneCompact(new GddlMap() { { "a b", GddlValue.Of(1) } }));
        }

        public static string FormatOneCompact(GddlElement e)
        {
            var b = new StringBuilder();
            new Formatter(b, FormatterOptions.Compact).FormatElement(e);
            return b.ToString();
        }
    }
}