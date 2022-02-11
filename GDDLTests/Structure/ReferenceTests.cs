using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDDL.Tests.Structure
{
    [TestClass]
    public class ReferenceTests
    {
        [TestMethod]
        public void ReferenceResolvesToElement()
        {
            Collection root = Collection.Of(GddlValue.Of("child"));
            GddlReference r = GddlReference.Absolute();
            r.Resolve(root, root);
            Assert.AreEqual(root, r.ResolvedValue);
        }

        [TestMethod]
        public void AbsoluteReferenceResolvesToRoot()
        {
            Collection parent = Collection.Of(GddlValue.Of("parent"));
            Collection root = Collection.Of(GddlValue.Of("root"), parent);
            GddlReference r = GddlReference.Absolute();
            r.Resolve(root, parent);
            Assert.AreEqual(root, r.ResolvedValue);
        }

        [TestMethod]
        public void RelativeReferenceResolvesToParent()
        {
            Collection parent = Collection.Of(GddlValue.Of("parent"));
            Collection root = Collection.Of(GddlValue.Of("root"), parent);
            GddlReference r = GddlReference.Relative();
            r.Resolve(root, parent);
            Assert.AreEqual(parent, r.ResolvedValue);
        }

        [TestMethod]
        public void AbsoluteChildReferenceResolvesToChild()
        {
            var relativeChild = GddlValue.Of("Relative child").WithName("child");
            var absoluteChild = GddlValue.Of("Absolute child").WithName("child");
            Collection parent = Collection.Of(GddlValue.Of("parent"), relativeChild);
            Collection root = Collection.Of(GddlValue.Of("root"), absoluteChild, parent);
            GddlReference r = GddlReference.Absolute("child");
            r.Resolve(root, parent);
            Assert.AreEqual(absoluteChild, r.ResolvedValue);
        }

        [TestMethod]
        public void RelativeChildReferenceResolvesToChild()
        {
            var relativeChild = GddlValue.Of("Relative child").WithName("child");
            var absoluteChild = GddlValue.Of("Absolute child").WithName("child");
            Collection parent = Collection.Of(GddlValue.Of("parent"), relativeChild);
            Collection root = Collection.Of(GddlValue.Of("root"), absoluteChild, parent);
            GddlReference r = GddlReference.Relative("child");
            r.Resolve(root, parent);
            Assert.AreEqual(relativeChild, r.ResolvedValue);
        }

        [TestMethod]
        public void NestedChildReferenceResolvesToChild()
        {
            var relativeChild = GddlValue.Of("the child").WithName("child");
            var parent = Collection.Of(GddlValue.Of("the parent"), relativeChild).WithName("parent");
            var root = Collection.Of(GddlValue.Of("root"), parent);
            GddlReference r = GddlReference.Relative("parent", "child");
            r.Resolve(root, parent);
            Assert.AreEqual(relativeChild, r.ResolvedValue);
        }
    }
}
