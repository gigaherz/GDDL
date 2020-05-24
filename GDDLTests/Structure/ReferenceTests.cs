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
        public void referenceResolvesToElement()
        {
            Collection root = Collection.Of(Value.Of("child"));
            Reference r = Reference.Absolute();
            r.Resolve(root, root);
            Assert.AreEqual(root, r.ResolvedValue);
        }

        [TestMethod]
        public void absoluteReferenceResolvesToRoot()
        {
            Collection parent = Collection.Of(Value.Of("parent"));
            Collection root = Collection.Of(Value.Of("root"), parent);
            Reference r = Reference.Absolute();
            r.Resolve(root, parent);
            Assert.AreEqual(root, r.ResolvedValue);
        }

        [TestMethod]
        public void relativeReferenceResolvesToParent()
        {
            Collection parent = Collection.Of(Value.Of("parent"));
            Collection root = Collection.Of(Value.Of("root"), parent);
            Reference r = Reference.Relative();
            r.Resolve(root, parent);
            Assert.AreEqual(parent, r.ResolvedValue);
        }

        [TestMethod]
        public void absoluteChildReferenceResolvesToChild()
        {
            var relativeChild = Value.Of("Relative child").WithName("child");
            var absoluteChild = Value.Of("Absolute child").WithName("child");
            Collection parent = Collection.Of(Value.Of("parent"), relativeChild);
            Collection root = Collection.Of(Value.Of("root"), absoluteChild, parent);
            Reference r = Reference.Absolute("child");
            r.Resolve(root, parent);
            Assert.AreEqual(absoluteChild, r.ResolvedValue);
        }

        [TestMethod]
        public void relativeChildReferenceResolvesToChild()
        {
            var relativeChild = Value.Of("Relative child").WithName("child");
            var absoluteChild = Value.Of("Absolute child").WithName("child");
            Collection parent = Collection.Of(Value.Of("parent"), relativeChild);
            Collection root = Collection.Of(Value.Of("root"), absoluteChild, parent);
            Reference r = Reference.Relative("child");
            r.Resolve(root, parent);
            Assert.AreEqual(relativeChild, r.ResolvedValue);
        }

        [TestMethod]
        public void nestedChildReferenceResolvesToChild()
        {
            var relativeChild = Value.Of("the child").WithName("child");
            var parent = Collection.Of(Value.Of("the parent"), relativeChild).WithName("parent");
            var root = Collection.Of(Value.Of("root"), parent);
            Reference r = Reference.Relative("parent", "child");
            r.Resolve(root, parent);
            Assert.AreEqual(relativeChild, r.ResolvedValue);
        }
    }
}
