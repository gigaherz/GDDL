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
        public void MockLexerWorks()
        {
            ITokenProvider provider = LexerBuilder()
                    .AddInt()
                    .AddNInt()
                    .AddFloat()
                    .AddNFloat()
                    .AddNaN()
                    .AddInf()
                    .AddNInf()
                    .AddHexInt()
                    .AddString()
                    .AddBooleanTrue()
                    .AddBooleanFalse()
                    .AddLBrace()
                    .AddRBrace()
                    .AddEquals()
                    .AddColon()
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
        public void ParsesNullAsValue()
        {
            ITokenProvider provider = LexerBuilder().AddInt().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(1), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesIntegerAsValue()
        {
            ITokenProvider provider = LexerBuilder().AddInt().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(1), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesNegativeIntegerAsValue()
        {
            ITokenProvider provider = LexerBuilder().AddNInt().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(-1), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesHexIntegerAsValue()
        {
            ITokenProvider provider = LexerBuilder().AddHexInt().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(1), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesDoubleAsValue()
        {
            var provider = LexerBuilder().AddFloat().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(1.0), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesNegativeDoubleAsValue()
        {
            var provider = LexerBuilder().AddNFloat().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(-1.0), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesNaNAsValue()
        {
            var provider = LexerBuilder().AddNaN().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(double.NaN), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesInfinityAsValue()
        {
            var provider = LexerBuilder().AddInf().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(double.PositiveInfinity), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesNegativeInfinityAsValue()
        {
            var provider = LexerBuilder().AddNInf().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(double.NegativeInfinity), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesStringAsValue()
        {
            var provider = LexerBuilder().AddString().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of("1"), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesBooleanTrueAsValue()
        {
            var provider = LexerBuilder().AddBooleanTrue().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(true), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesBooleanFalseAsValue()
        {
            var provider = LexerBuilder().AddBooleanFalse().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Value.Of(false), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesBracesAsCollection()
        {
            var provider = LexerBuilder().AddLBrace().AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Collection.Empty(), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesTypedCollection()
        {
            var provider = LexerBuilder().AddIdentifier("test").AddLBrace().AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Collection.Empty().WithTypeName("test"), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesValueInsideCollection()
        {
            var provider = LexerBuilder().AddLBrace().AddInt().AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Collection.Of(Value.Of(1)), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesMultipleValuesInsideCollection()
        {
            var provider = LexerBuilder().AddLBrace().AddInt().AddComma().AddInt().AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Collection.Of(Value.Of(1), Value.Of(1)), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesNestedCollection()
        {
            var provider = LexerBuilder().AddLBrace().AddLBrace().AddRBrace().AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Collection expected = Collection.Of(Collection.Empty());
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CommaIsOptionalAfterNestedCollection()
        {
            var provider = LexerBuilder().AddLBrace().AddLBrace().AddRBrace().AddInt().AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Collection expected = Collection.Of(Collection.Empty(), Value.Of(1));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AcceptsTrailingCommaInCollection()
        {
            ITokenProvider provider = LexerBuilder().AddLBrace().AddInt().AddComma().AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Assert.AreEqual(Collection.Of(Value.Of(1)), parser.Parse(false));
        }

        [TestMethod]
        public void ParsesNamedValueInsideCollection()
        {
            var provider = LexerBuilder().AddLBrace().AddString("\"a\"").AddEquals().AddInt().AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Collection expected = Collection.Of(Value.Of(1).WithName("a"));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParsesNamedTypedNestedCollection()
        {
            var provider = LexerBuilder().AddLBrace().AddString("\"n\"").AddEquals().AddIdentifier("a").AddLBrace().AddRBrace().AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Collection expected = Collection.Of(Collection.Empty().WithTypeName("a").WithName("n"));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParsesReference()
        {
            var provider = LexerBuilder().AddLBrace().AddIdentifier("a").AddColon().AddIdentifier("b").AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Collection expected = Collection.Of(Reference.Relative("a", "b"));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParsesRootedReference()
        {
            var provider = LexerBuilder().AddLBrace().AddColon().AddIdentifier("a").AddColon().AddIdentifier("b").AddRBrace().Build();
            Parser parser = Parser.FromProvider(provider);
            Collection expected = Collection.Of(Reference.Absolute("a", "b"));
            Element actual = parser.Parse(false);
            Assert.AreEqual(expected, actual);
        }

        // HARNESS BELOW
        // -------------

        private static MockLexerBuilder LexerBuilder()
        {
            return new MockLexerBuilder();
        }

        private class MockLexerBuilder
        {
            private readonly List<Token> builder = new List<Token>();

            public MockLexerBuilder AddEnd()
            {
                return Add(TokenType.End, "");
            }

            public MockLexerBuilder AddInt()
            {
                return Add(TokenType.Integer, "1");
            }

            public MockLexerBuilder AddNInt()
            {
                return Add(TokenType.Integer, "-1");
            }

            public MockLexerBuilder AddFloat()
            {
                return Add(TokenType.Double, "1.0");
            }

            public MockLexerBuilder AddNFloat()
            {
                return Add(TokenType.Double, "-1.0");
            }

            public MockLexerBuilder AddNaN()
            {
                return Add(TokenType.Double, ".NaN");
            }

            public MockLexerBuilder AddInf()
            {
                return Add(TokenType.Double, ".Inf");
            }

            public MockLexerBuilder AddNInf()
            {
                return Add(TokenType.Double, "-.Inf");
            }

            public MockLexerBuilder AddHexInt()
            {
                return Add(TokenType.HexInt, "0x1");
            }

            public MockLexerBuilder AddString()
            {
                return Add(TokenType.String, "\"1\"");
            }

            public MockLexerBuilder AddString(string text)
            {
                return Add(TokenType.String, text);
            }

            public MockLexerBuilder AddBooleanTrue()
            {
                return Add(TokenType.True, "true");
            }

            public MockLexerBuilder AddBooleanFalse()
            {
                return Add(TokenType.False, "false");
            }

            public MockLexerBuilder AddLBrace()
            {
                return Add(TokenType.LBrace, "{");
            }

            public MockLexerBuilder AddRBrace()
            {
                return Add(TokenType.RBrace, "}");
            }

            public MockLexerBuilder AddColon()
            {
                return Add(TokenType.Colon, ":");
            }

            public MockLexerBuilder AddComma()
            {
                return Add(TokenType.Comma, ",");
            }

            public MockLexerBuilder AddEquals()
            {
                return Add(TokenType.EqualSign, "=");
            }

            public MockLexerBuilder AddIdentifier()
            {
                return Add(TokenType.Ident, "test");
            }

            public MockLexerBuilder AddIdentifier(string text)
            {
                return Add(TokenType.Ident, text);
            }

            public MockLexerBuilder Add(TokenType name, string text)
            {
                return Add(name, text, new ParsingContext("TEST", 1, 1));
            }

            public MockLexerBuilder Add(TokenType name, string text, ParsingContext context)
            {
                return Add(name, text, context, "");
            }

            public MockLexerBuilder Add(TokenType name, string text, ParsingContext context, string comment)
            {
                builder.Add(new Token(name, text, context, comment));
                return this;
            }

            public MockLexer Build()
            {
                AddEnd();
                return new MockLexer(builder.AsReadOnly());
            }
        }

        private sealed class MockLexer : ITokenProvider
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

            public ParsingContext ParsingContext
            {
                get
                {
                    if (closed)
                        throw new InvalidOperationException("The ITokenProvider is closed.");
                    return preparedTokens[Math.Min(index, preparedTokens.Count - 1)].ParsingContext;
                }
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