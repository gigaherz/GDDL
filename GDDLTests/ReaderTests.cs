﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using GDDL;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GDDL.Tests
{
    [TestClass]
    public class ReaderTests
    {
        public const string SOURCE_NAME = "TEST";

        [TestMethod]
        public void parsingContextEqualsWorks()
        {
            ParsingContext @base = new ParsingContext("A", 2, 3);
            ParsingContext same = new ParsingContext("A", 2, 3);
            ParsingContext differentFile = new ParsingContext("B", 2, 3);
            ParsingContext differentLine = new ParsingContext("A", 1, 3);
            ParsingContext differentColumn = new ParsingContext("A", 2, 5);
            Assert.AreEqual(@base, same);
            Assert.AreNotEqual(@base, differentFile);
            Assert.AreNotEqual(@base, differentLine);
            Assert.AreNotEqual(@base, differentColumn);
        }

        [TestMethod]
        public void readsOneCharacter()
        {
            String testString = "a";
            Reader reader = new Reader(new StringReader(testString), SOURCE_NAME);
            Assert.AreEqual(testString, reader.Read(testString.Length));
        }

        [TestMethod]
        public void readsMultipleCharacters()
        {
            String testString = "qwerty";
            Reader reader = new Reader(new StringReader(testString), SOURCE_NAME);
            Assert.AreEqual(testString, reader.Read(testString.Length));
        }

        [TestMethod]
        public void readsOnlyAsMuchAsRequested()
        {
            Reader reader = new Reader(new StringReader("qwerty"), SOURCE_NAME);
            Assert.AreEqual("qwe", reader.Read(3));
        }

        [TestMethod]
        public void peekWorks()
        {
            Reader reader = new Reader(new StringReader("abc"), SOURCE_NAME);

            Assert.AreEqual('a', reader.Peek());
            Assert.AreEqual('a', reader.Peek(0));
            Assert.AreEqual('b', reader.Peek(1));
            Assert.AreEqual('c', reader.Peek(2));

            Assert.AreEqual("a", reader.Read(1));

            Assert.AreEqual('b', reader.Peek());
            Assert.AreEqual('b', reader.Peek(0));
            Assert.AreEqual('c', reader.Peek(1));

            Assert.AreEqual("b", reader.Read(1));
        }

        [TestMethod]
        public void peeksAfterRead()
        {
            Reader reader = new Reader(new StringReader("zxcvbnm"), SOURCE_NAME);
            Assert.AreEqual("zxc", reader.Read(3));
            Assert.AreEqual('v', reader.Peek());
        }

        [TestMethod]
        public void peeksEndOfFile()
        {
            Reader reader = new Reader(new StringReader(""), SOURCE_NAME);
            Assert.AreEqual(-1, reader.Peek());
        }

        [TestMethod]
        public void keepsTrackOfLocation()
        {
            Reader reader = new Reader(new StringReader("qwerty\nuiop\rasdf\r\n1234"), SOURCE_NAME);
            Assert.AreEqual(new ParsingContext(SOURCE_NAME, 1, 1), reader.GetParsingContext());
            Assert.AreEqual("qw", reader.Read(2));
            Assert.AreEqual(new ParsingContext(SOURCE_NAME, 1, 3), reader.GetParsingContext());
            Assert.AreEqual("erty\nuio", reader.Read(8));
            Assert.AreEqual(new ParsingContext(SOURCE_NAME, 2, 4), reader.GetParsingContext());
            Assert.AreEqual("p\rasdf\r\n", reader.Read(8));
            Assert.AreEqual(new ParsingContext(SOURCE_NAME, 4, 1), reader.GetParsingContext());
            Assert.AreEqual("1234", reader.Read(4));
            Assert.AreEqual(-1, reader.Peek());
        }
    }
}