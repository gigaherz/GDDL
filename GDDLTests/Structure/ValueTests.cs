using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDDL.Tests.Structure
{
    [TestClass]
    public class ValueTests
    {
        [TestMethod]
        public void EqualsAndHashcodeWork()
        {
            GddlValue vNull = GddlValue.Null();
            GddlValue vSame1 = GddlValue.Of(1);
            GddlValue vSame2 = GddlValue.Of(1);
            GddlValue vDifferent = GddlValue.Of("s");
            Assert.AreEqual(vSame1, vSame2);
            Assert.AreEqual(vSame2, vSame1);
            Assert.AreNotEqual(vSame1, vNull);
            Assert.AreNotEqual(vNull, vSame1);
            Assert.AreNotEqual(vSame1, vDifferent);
            Assert.AreNotEqual(vDifferent, vSame1);
            Assert.AreEqual(vSame1.GetHashCode(), vSame2.GetHashCode());
            // No I can't test that the hash codes are different for the other cases, because they don't need to be.
        }

        [TestMethod]
        public void NullValueWorks()
        {
            GddlValue v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            Assert.ThrowsException<NullReferenceException>(() => v.Boolean);
            Assert.ThrowsException<NullReferenceException>(() => v.Integer);
            Assert.ThrowsException<NullReferenceException>(() => v.Double);
            Assert.ThrowsException<NullReferenceException>(() => v.String);
        }

        [TestMethod]
        public void OfBooleanTrueWorks()
        {
            GddlValue v = GddlValue.Of(true);
            Assert.IsFalse(v.IsNull);
            Assert.IsTrue(v.Boolean);
            Assert.ThrowsException<InvalidCastException>(() => v.Integer);
            Assert.ThrowsException<InvalidCastException>(() => v.Double);
            Assert.ThrowsException<InvalidCastException>(() => v.String);
        }

        [TestMethod]
        public void OfBooleanFalseWorks()
        {
            GddlValue v = GddlValue.Of(false);
            Assert.IsFalse(v.IsNull);
            Assert.IsFalse(v.Boolean);
            Assert.ThrowsException<InvalidCastException>(() => v.Integer);
            Assert.ThrowsException<InvalidCastException>(() => v.Double);
            Assert.ThrowsException<InvalidCastException>(() => v.String);
        }

        [TestMethod]
        public void OfLongWorks()
        {
            GddlValue v = GddlValue.Of(1);
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1L, v.Integer);
            Assert.ThrowsException<InvalidCastException>(() => v.Boolean);
            Assert.ThrowsException<InvalidCastException>(() => v.Double);
            Assert.ThrowsException<InvalidCastException>(() => v.String);
        }

        [TestMethod]
        public void OfDoubleWorks()
        {
            GddlValue v = GddlValue.Of(1.0);
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1L, v.Double, 1E-10);
            Assert.ThrowsException<InvalidCastException>(() => v.Boolean);
            Assert.ThrowsException<InvalidCastException>(() => v.Integer);
            Assert.ThrowsException<InvalidCastException>(() => v.String);
        }

        [TestMethod]
        public void OfStringWorks()
        {
            GddlValue v = GddlValue.Of("1");
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual("1", v.String);
            Assert.ThrowsException<InvalidCastException>(() => v.Boolean);
            Assert.ThrowsException<InvalidCastException>(() => v.Integer);
            Assert.ThrowsException<InvalidCastException>(() => v.Double);
        }

        [TestMethod]
        public void CopyOfNullWorks()
        {
            GddlValue v = GddlValue.Null().Copy();
            Assert.IsTrue(v.IsNull);
        }

        [TestMethod]
        public void CopyOfBooleanTrueWorks()
        {
            GddlValue v = GddlValue.Of(true).Copy();
            Assert.IsFalse(v.IsNull);
            Assert.IsTrue(v.Boolean);
        }

        [TestMethod]
        public void CopyOfBooleanFalseWorks()
        {
            GddlValue v = GddlValue.Of(false).Copy();
            Assert.IsFalse(v.IsNull);
            Assert.IsFalse(v.Boolean);
        }

        [TestMethod]
        public void CopyOfLongWorks()
        {
            GddlValue v = GddlValue.Of(1).Copy();
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1L, v.Integer);
        }

        [TestMethod]
        public void CopyOfDoubleWorks()
        {
            GddlValue v = GddlValue.Of(1.0).Copy();
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1L, v.Double, 1E-10);
        }

        [TestMethod]
        public void CopyOfStringWorks()
        {
            GddlValue v = (GddlValue)GddlValue.Of("1").CopyInternal();
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual("1", v.String);
        }

        [TestMethod]
        public void SetNullWorks()
        {
            GddlValue v = GddlValue.Of(1);
            Assert.IsFalse(v.IsNull);
            v.SetNull();
            Assert.IsTrue(v.IsNull);
        }

        [TestMethod]
        public void SetBooleanWorks()
        {
            GddlValue v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            v.Boolean = false;
            Assert.IsFalse(v.IsNull);
            Assert.IsFalse(v.Boolean);
        }

        [TestMethod]
        public void SetLongWorks()
        {
            GddlValue v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            v.Integer = 1;
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1, v.Integer);
        }

        [TestMethod]
        public void SetDoubleWorks()
        {
            GddlValue v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            v.Double = 1;
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1, v.Double, 1E-10);
        }

        [TestMethod]
        public void SetStringWorks()
        {
            GddlValue v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            v.String = "a";
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual("a", v.String);
        }
    }
}
