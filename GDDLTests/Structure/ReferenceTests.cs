﻿using GDDL.Structure;
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
            GddlMap root = new GddlMap() { { "child", GddlValue.Of("child") } };
            GddlReference r = GddlReference.Absolute();
            r.Resolve(root);
            Assert.AreEqual(root, r.ResolvedValue);
        }

        [TestMethod]
        public void AbsoluteChildReferenceResolvesToChild()
        {
            var relativeChild = GddlValue.Of("Relative child");
            var absoluteChild = GddlValue.Of("Absolute child");
            GddlMap parent = new GddlMap() { { "parent", GddlValue.Of("parent") }, { "child", relativeChild } };
            GddlMap root = new GddlMap() { { "root", GddlValue.Of("root") }, { "child", absoluteChild }, { "parent", parent } };
            GddlReference r = GddlReference.Absolute("child");
            parent.Add("reference", r);
            r.Resolve(root);
            Assert.AreEqual(absoluteChild, r.ResolvedValue);
        }

        [TestMethod]
        public void RelativeChildReferenceResolvesToChild()
        {
            var relativeChild = GddlValue.Of("Relative child");
            var absoluteChild = GddlValue.Of("Absolute child");
            GddlMap parent = new GddlMap() { { "parent", GddlValue.Of("parent") }, { "child", relativeChild } };
            GddlMap root = new GddlMap() { { "root", GddlValue.Of("root") }, { "child", absoluteChild }, { "parent", parent } };
            GddlReference r = GddlReference.Relative("child");
            parent.Add("reference", r);
            r.Resolve(root);
            Assert.AreEqual(relativeChild, r.ResolvedValue);
        }

        [TestMethod]
        public void NestedChildReferenceResolvesToChild()
        {
            var relativeChild = GddlValue.Of("Relative child");
            var absoluteChild = GddlValue.Of("Absolute child");
            GddlMap parent = new GddlMap() { { "parent", GddlValue.Of("parent") }, { "child", relativeChild } };
            GddlMap root = new GddlMap() { { "root", GddlValue.Of("root") }, { "child", absoluteChild }, { "parent", parent } };
            GddlReference r = GddlReference.Relative("parent", "child");
            parent.Add("reference", r);
            r.Resolve(root);
            Assert.AreEqual(relativeChild, r.ResolvedValue);
        }
    }
}
