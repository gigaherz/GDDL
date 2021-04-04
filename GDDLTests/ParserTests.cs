using Microsoft.VisualStudio.TestTools.UnitTesting;
using GDDL;
using System;
using System.Collections.Generic;
using System.Text;
using GDDL.Structure;

namespace GDDL.Tests
{
    [TestClass]
    public class ParserTests
    {

        [TestMethod]
        public void mockLexerWorks()
        {
            ITokenProvider provider = lexerBuilder()
                    .addInt()
                    .addNInt()
                    .addFloat()
                    .addNFloat()
                    .addNaN()
                    .addInf()
                    .addNInf()
                    .addHexInt()
                    .addString()
                    .addBooleanTrue()
                    .addBooleanFalse()
                    .addLBrace()
                    .addRBrace()
                    .addEquals()
                    .addColon()
                    .Build();

            Assert.AreEqual(new Token(TokenType.Integer, "1", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.Integer, "-1", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.Double, "1.0", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.Double, "-1.0", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.Double, ".NaN", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.Double, ".Inf", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.Double, "-.Inf", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.HexInt, "0x1", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.String, "\"1\"", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.True, "true", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.False, "false", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.LBrace, "{", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.RBrace, "}", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.EqualSign, "=", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.Colon, ":", new ParsingContext("TEST", 1, 1), ""), provider.Pop());

            // Test that it automatically returns an End token at the end.
            Assert.AreEqual(new Token(TokenType.End, "", new ParsingContext("TEST", 1, 1), ""), provider.Pop());

            // Test that you can continue popping End tokens even after the end.
            Assert.AreEqual(new Token(TokenType.End, "", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.End, "", new ParsingContext("TEST", 1, 1), ""), provider.Pop());
        }

        [TestMethod]
        public void parsesNullAsValue()
        {
            ITokenProvider provider = lexerBuilder().addInt().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(1), parser.Parse(false));
        }

        [TestMethod]
        public void parsesIntegerAsValue()
        {
            ITokenProvider provider = lexerBuilder().addInt().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(1), parser.Parse(false));
        }

        [TestMethod]
        public void parsesNegativeIntegerAsValue()
        {
            ITokenProvider provider = lexerBuilder().addNInt().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(-1), parser.Parse(false));
        }

        [TestMethod]
        public void parsesHexIntegerAsValue()
        {
            ITokenProvider provider = lexerBuilder().addHexInt().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(1), parser.Parse(false));
        }

        [TestMethod]
        public void parsesDoubleAsValue()
        {
            var provider = lexerBuilder().addFloat().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(1.0), parser.Parse(false));
        }

        [TestMethod]
        public void parsesNegativeDoubleAsValue()
        {
            var provider = lexerBuilder().addNFloat().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(-1.0), parser.Parse(false));
        }

        [TestMethod]
        public void parsesNaNAsValue()
        {
            var provider = lexerBuilder().addNaN().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(double.NaN), parser.Parse(false));
        }

        [TestMethod]
        public void parsesInfinityAsValue()
        {
            var provider = lexerBuilder().addInf().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(double.PositiveInfinity), parser.Parse(false));
        }

        [TestMethod]
        public void parsesNegativeInfinityAsValue()
        {
            var provider = lexerBuilder().addNInf().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(double.NegativeInfinity), parser.Parse(false));
        }

        [TestMethod]
        public void parsesStringAsValue()
        {
            var provider = lexerBuilder().addString().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of("1"), parser.Parse(false));
        }

        [TestMethod]
        public void parsesBooleanTrueAsValue()
        {
            var provider = lexerBuilder().addBooleanTrue().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(true), parser.Parse(false));
        }

        [TestMethod]
        public void parsesBooleanFalseAsValue()
        {
            var provider = lexerBuilder().addBooleanFalse().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Value.Of(false), parser.Parse(false));
        }

        [TestMethod]
        public void parsesBracesAsCollection()
        {
            var provider = lexerBuilder().addLBrace().addRBrace().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Collection.Empty(), parser.Parse(false));
        }

        [TestMethod]
        public void parsesTypedCollection()
        {
            var provider = lexerBuilder().addIdentifier("test").addLBrace().addRBrace().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Collection.Empty().WithTypeName("test"), parser.Parse(false));
        }

        [TestMethod]
        public void parsesValueInsideCollection()
        {
            var provider = lexerBuilder().addLBrace().addInt().addRBrace().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Collection.Of(Value.Of(1)), parser.Parse(false));
        }

        [TestMethod]
        public void parsesMultipleValuesInsideCollection()
        {
            var provider = lexerBuilder().addLBrace().addInt().addComma().addInt().addRBrace().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Collection.Of(Value.Of(1), Value.Of(1)), parser.Parse(false));
        }

        [TestMethod]
        public void parsesNestedCollection()
        {
            var provider = lexerBuilder().addLBrace().addLBrace().addRBrace().addRBrace().Build();
            Parser parser = new Parser(provider);
            Collection expected = Collection.Of(Collection.Empty());
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void commaIsOptionalAfterNestedCollection()
        {
            var provider = lexerBuilder().addLBrace().addLBrace().addRBrace().addInt().addRBrace().Build();
            Parser parser = new Parser(provider);
            Collection expected = Collection.Of(Collection.Empty(), Value.Of(1));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void acceptsTrailingCommaInCollection()
        {
            ITokenProvider provider = lexerBuilder().addLBrace().addInt().addComma().addRBrace().Build();
            Parser parser = new Parser(provider);
            Assert.AreEqual(Collection.Of(Value.Of(1)), parser.Parse(false));
        }

        [TestMethod]
        public void parsesNamedValueInsideCollection()
        {
            var provider = lexerBuilder().addLBrace().addString("\"a\"").addEquals().addInt().addRBrace().Build();
            Parser parser = new Parser(provider);
            Collection expected = Collection.Of(Value.Of(1).WithName("a"));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void parsesNamedTypedNestedCollection()
        {
            var provider = lexerBuilder().addLBrace().addString("\"n\"").addEquals().addIdentifier("a").addLBrace().addRBrace().addRBrace().Build();
            Parser parser = new Parser(provider);
            Collection expected = Collection.Of(Collection.Empty().WithTypeName("a").WithName("n"));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void parsesBackreference()
        {
            var provider = lexerBuilder().addLBrace().addIdentifier("a").addColon().addIdentifier("b").addRBrace().Build();
            Parser parser = new Parser(provider);
            Collection expected = Collection.Of(Reference.Relative("a", "b"));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void parsesRootedBackreference()
        {
            var provider = lexerBuilder().addLBrace().addColon().addIdentifier("a").addColon().addIdentifier("b").addRBrace().Build();
            Parser parser = new Parser(provider);
            Collection expected = Collection.Of(Reference.Absolute("a", "b"));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        // HARNESS BELOW
        // -------------

        public static MockLexerBuilder lexerBuilder()
        {
            return new MockLexerBuilder();
        }

        public class MockLexerBuilder
        {
            private readonly List<Token> builder = new List<Token>();

            public MockLexerBuilder addEnd()
            {
                return add(TokenType.End, "");
            }

            public MockLexerBuilder addInt()
            {
                return add(TokenType.Integer, "1");
            }

            public MockLexerBuilder addNInt()
            {
                return add(TokenType.Integer, "-1");
            }

            public MockLexerBuilder addFloat()
            {
                return add(TokenType.Double, "1.0");
            }

            public MockLexerBuilder addNFloat()
            {
                return add(TokenType.Double, "-1.0");
            }

            public MockLexerBuilder addNaN()
            {
                return add(TokenType.Double, ".NaN");
            }

            public MockLexerBuilder addInf()
            {
                return add(TokenType.Double, ".Inf");
            }

            public MockLexerBuilder addNInf()
            {
                return add(TokenType.Double, "-.Inf");
            }

            public MockLexerBuilder addHexInt()
            {
                return add(TokenType.HexInt, "0x1");
            }

            public MockLexerBuilder addString()
            {
                return add(TokenType.String, "\"1\"");
            }

            public MockLexerBuilder addString(String text)
            {
                return add(TokenType.String, text);
            }

            public MockLexerBuilder addBooleanTrue()
            {
                return add(TokenType.True, "true");
            }

            public MockLexerBuilder addBooleanFalse()
            {
                return add(TokenType.False, "false");
            }

            public MockLexerBuilder addLBrace()
            {
                return add(TokenType.LBrace, "{");
            }

            public MockLexerBuilder addRBrace()
            {
                return add(TokenType.RBrace, "}");
            }

            public MockLexerBuilder addColon()
            {
                return add(TokenType.Colon, ":");
            }

            public MockLexerBuilder addComma()
            {
                return add(TokenType.Comma, ",");
            }

            public MockLexerBuilder addEquals()
            {
                return add(TokenType.EqualSign, "=");
            }

            public MockLexerBuilder addIdentifier()
            {
                return add(TokenType.Ident, "test");
            }

            public MockLexerBuilder addIdentifier(String text)
            {
                return add(TokenType.Ident, text);
            }

            public MockLexerBuilder add(TokenType name, String text)
            {
                return add(name, text, new ParsingContext("TEST", 1, 1));
            }

            public MockLexerBuilder add(TokenType name, String text, ParsingContext context)
            {
                return add(name, text, context, "");
            }

            public MockLexerBuilder add(TokenType name, String text, ParsingContext context, String comment)
            {
                builder.Add(new Token(name, text, context, comment));
                return this;
            }

            public MockLexer Build()
            {
                addEnd();
                return new MockLexer(builder.AsReadOnly());
            }
        }

        public class MockLexer : ITokenProvider
        {
            public readonly IReadOnlyList<Token> preparedTokens;
            public int index = 0;
            public bool closed = false;

            public MockLexer(IReadOnlyList<Token> preparedTokens)
            {
                this.preparedTokens = preparedTokens;
            }

            private Token Get(int index)
            {
                if (closed)
                    throw new InvalidOperationException("The ITokenProvider is closed.");
                int idx = this.index + index;
                if (idx >= preparedTokens.Count)
                    idx = preparedTokens.Count - 1;
                return preparedTokens[idx];
            }

            public TokenType Peek()
            {
                return Peek(0);
            }

            public TokenType Peek(int index)
            {
                return Get(index).Type;
            }

            public Token Pop()
            {
                Token t = Get(0);
                index++;
                return t;
            }

            public ParsingContext GetParsingContext()
            {
                if (closed)
                    throw new InvalidOperationException("The ITokenProvider is closed.");
                return preparedTokens[Math.Min(index, preparedTokens.Count - 1)].Context;
            }

            public void Dispose()
            {
                if (closed)
                    throw new InvalidOperationException("The ITokenProvider has already been closed before.");
                closed = true;
            }
        }
    }
}