using GDDL.Queries;
using GDDL.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GDDL.Tests.Structure
{
    [TestClass]
    public class ReferenceTests
    {
        [TestMethod]
        public void ReferenceResolvesToElement()
        {
            var root = new GddlMap() { { "child", GddlValue.Of("child") } };
            var r = GddlReference.Of(new Query().Absolute());
            r.Resolve(root);
            Assert.AreEqual(root, r.ResolvedValue);
        }

        [TestMethod]
        public void AbsoluteChildReferenceResolvesToChild()
        {
            var relativeChild = GddlValue.Of("Relative child");
            var absoluteChild = GddlValue.Of("Absolute child");
            var parent = new GddlMap() { { "parent", GddlValue.Of("parent") }, { "child", relativeChild } };
            var root = new GddlMap()
                { { "root", GddlValue.Of("root") }, { "child", absoluteChild }, { "parent", parent } };
            var r = GddlReference.Of(new Query().Absolute().ByKey("child"));
            parent.Add("reference", r);
            r.Resolve(root);
            Assert.AreEqual(absoluteChild, r.ResolvedValue);
        }

        [TestMethod]
        public void RelativeChildReferenceResolvesToChild()
        {
            var relativeChild = GddlValue.Of("Relative child");
            var absoluteChild = GddlValue.Of("Absolute child");
            var parent = new GddlMap() { { "parent", GddlValue.Of("parent") }, { "child", relativeChild } };
            var root = new GddlMap()
                { { "root", GddlValue.Of("root") }, { "child", absoluteChild }, { "parent", parent } };
            var r = GddlReference.Of(new Query().ByKey("child"));
            parent.Add("reference", r);
            r.Resolve(root);
            Assert.AreEqual(relativeChild, r.ResolvedValue);
        }

        [TestMethod]
        public void NestedChildReferenceResolvesToChild()
        {
            var relativeChild = GddlValue.Of("Relative child");
            var absoluteChild = GddlValue.Of("Absolute child");
            var parent2 = new GddlMap() { { "parent", GddlValue.Of("parent") }, { "child", relativeChild } };
            var parent = new GddlMap() { { "parent", parent2 } };
            var root = new GddlMap()
                { { "root", GddlValue.Of("root") }, { "child", absoluteChild }, { "parent", parent } };
            var r = GddlReference.Of(new Query().ByKey("parent").ByKey("child"));
            parent.Add("reference", r);
            r.Resolve(root);
            Assert.AreEqual(relativeChild, r.ResolvedValue);
        }
    }
}