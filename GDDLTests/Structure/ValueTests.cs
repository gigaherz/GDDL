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
            var vNull = GddlValue.Null();
            var vSame1 = GddlValue.Of(1);
            var vSame2 = GddlValue.Of(1);
            var vDifferent = GddlValue.Of("s");
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
            var v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            Assert.ThrowsException<NullReferenceException>(() => v.AsBoolean);
            Assert.ThrowsException<NullReferenceException>(() => v.AsInteger);
            Assert.ThrowsException<NullReferenceException>(() => v.AsDouble);
            Assert.ThrowsException<NullReferenceException>(() => v.AsString);
        }

        [TestMethod]
        public void OfBooleanTrueWorks()
        {
            var v = GddlValue.Of(true);
            Assert.IsFalse(v.IsNull);
            Assert.IsTrue(v.AsBoolean);
            Assert.ThrowsException<InvalidCastException>(() => v.AsInteger);
            Assert.ThrowsException<InvalidCastException>(() => v.AsDouble);
            Assert.ThrowsException<InvalidCastException>(() => v.AsString);
        }

        [TestMethod]
        public void OfBooleanFalseWorks()
        {
            var v = GddlValue.Of(false);
            Assert.IsFalse(v.IsNull);
            Assert.IsFalse(v.AsBoolean);
            Assert.ThrowsException<InvalidCastException>(() => v.AsInteger);
            Assert.ThrowsException<InvalidCastException>(() => v.AsDouble);
            Assert.ThrowsException<InvalidCastException>(() => v.AsString);
        }

        [TestMethod]
        public void OfLongWorks()
        {
            var v = GddlValue.Of(1);
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1L, v.AsInteger);
            Assert.ThrowsException<InvalidCastException>(() => v.AsBoolean);
            Assert.ThrowsException<InvalidCastException>(() => v.AsDouble);
            Assert.ThrowsException<InvalidCastException>(() => v.AsString);
        }

        [TestMethod]
        public void OfDoubleWorks()
        {
            var v = GddlValue.Of(1.0);
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1L, v.AsDouble, 1E-10);
            Assert.ThrowsException<InvalidCastException>(() => v.AsBoolean);
            Assert.ThrowsException<InvalidCastException>(() => v.AsInteger);
            Assert.ThrowsException<InvalidCastException>(() => v.AsString);
        }

        [TestMethod]
        public void OfStringWorks()
        {
            var v = GddlValue.Of("1");
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual("1", v.AsString);
            Assert.ThrowsException<InvalidCastException>(() => v.AsBoolean);
            Assert.ThrowsException<InvalidCastException>(() => v.AsInteger);
            Assert.ThrowsException<InvalidCastException>(() => v.AsDouble);
        }

        [TestMethod]
        public void CopyOfNullWorks()
        {
            var v = GddlValue.Null().Copy();
            Assert.IsTrue(v.IsNull);
        }

        [TestMethod]
        public void CopyOfBooleanTrueWorks()
        {
            var v = GddlValue.Of(true).Copy();
            Assert.IsFalse(v.IsNull);
            Assert.IsTrue(v.AsBoolean);
        }

        [TestMethod]
        public void CopyOfBooleanFalseWorks()
        {
            var v = GddlValue.Of(false).Copy();
            Assert.IsFalse(v.IsNull);
            Assert.IsFalse(v.AsBoolean);
        }

        [TestMethod]
        public void CopyOfLongWorks()
        {
            var v = GddlValue.Of(1).Copy();
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1L, v.AsInteger);
        }

        [TestMethod]
        public void CopyOfDoubleWorks()
        {
            var v = GddlValue.Of(1.0).Copy();
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1L, v.AsDouble, 1E-10);
        }

        [TestMethod]
        public void CopyOfStringWorks()
        {
            var v = GddlValue.Of("1").Copy();
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual("1", v.AsString);
        }

        [TestMethod]
        public void SetNullWorks()
        {
            var v = GddlValue.Of(1);
            Assert.IsFalse(v.IsNull);
            v.SetNull();
            Assert.IsTrue(v.IsNull);
        }

        [TestMethod]
        public void SetBooleanWorks()
        {
            var v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            v.SetBool(false);
            Assert.IsFalse(v.IsNull);
            Assert.IsFalse(v.AsBoolean);
        }

        [TestMethod]
        public void SetLongWorks()
        {
            var v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            v.SetInt(1);
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1, v.AsInteger);
        }

        [TestMethod]
        public void SetDoubleWorks()
        {
            var v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            v.SetDouble(1);
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual(1, v.AsDouble, 1E-10);
        }

        [TestMethod]
        public void SetStringWorks()
        {
            var v = GddlValue.Null();
            Assert.IsTrue(v.IsNull);
            v.SetString("a");
            Assert.IsFalse(v.IsNull);
            Assert.AreEqual("a", v.AsString);
        }
    }
}