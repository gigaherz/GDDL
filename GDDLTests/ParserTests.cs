using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using GDDL.Exceptions;
using GDDL.Parsing;
using GDDL.Queries;
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
                    .AddLBracket()
                    .AddRBracket()
                    .AddEquals()
                    .AddColon()
                    .AddSlash()
                    .Build();

            Assert.AreEqual(new Token(TokenType.IntegerLiteral, "1", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.IntegerLiteral, "-1", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.DecimalLiteral, "1.0", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.DecimalLiteral, "-1.0", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.DecimalLiteral, ".NaN", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.DecimalLiteral, ".Inf", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.DecimalLiteral, "-.Inf", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.HexIntLiteral, "0x1", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.StringLiteral, "\"1\"", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.True, "true", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.False, "false", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.LBrace, "{", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.RBrace, "}", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.LBracket, "[", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.RBracket, "]", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.EqualSign, "=", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.Colon, ":", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.Slash, "/", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());

            // Test that it automatically returns an End token at the end.
            Assert.AreEqual(new Token(TokenType.End, "", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());

            // Test that you can continue popping End tokens even after the end.
            Assert.AreEqual(new Token(TokenType.End, "", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
            Assert.AreEqual(new Token(TokenType.End, "", new ParsingContext("TEST", 1, 1), "", ""), provider.Pop());
        }

        [TestMethod]
        public void ParsesNullAsValue()
        {
            ITokenProvider provider = LexerBuilder().AddInt().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(1), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesIntegerAsValue()
        {
            ITokenProvider provider = LexerBuilder().AddInt().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(1), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesNegativeIntegerAsValue()
        {
            ITokenProvider provider = LexerBuilder().AddNInt().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(-1), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesHexIntegerAsValue()
        {
            ITokenProvider provider = LexerBuilder().AddHexInt().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(1), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesDoubleAsValue()
        {
            var provider = LexerBuilder().AddFloat().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(1.0), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesNegativeDoubleAsValue()
        {
            var provider = LexerBuilder().AddNFloat().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(-1.0), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesNaNAsValue()
        {
            var provider = LexerBuilder().AddNaN().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(double.NaN), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesInfinityAsValue()
        {
            var provider = LexerBuilder().AddInf().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(double.PositiveInfinity), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesNegativeInfinityAsValue()
        {
            var provider = LexerBuilder().AddNInf().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(double.NegativeInfinity), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesStringAsValue()
        {
            var provider = LexerBuilder().AddString().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of("1"), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesBooleanTrueAsValue()
        {
            var provider = LexerBuilder().AddBooleanTrue().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(true), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesBooleanFalseAsValue()
        {
            var provider = LexerBuilder().AddBooleanFalse().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlValue.Of(false), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesBracketsAsList()
        {
            var provider = LexerBuilder().AddLBracket().AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            var expected = GddlList.Empty();
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesValueInsideList()
        {
            var provider = LexerBuilder().AddLBracket().AddInt().AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            var expected = GddlList.Of(GddlValue.Of(1));
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesMultipleValuesInsideList()
        {
            var provider = LexerBuilder().AddLBracket().AddInt().AddComma().AddInt().AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            var expected = GddlList.Of(GddlValue.Of(1), GddlValue.Of(1));
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesNestedList()
        {
            var provider = LexerBuilder().AddLBracket().AddLBracket().AddRBracket().AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            GddlList expected = GddlList.Of(GddlList.Empty());
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void CommaIsOptionalAfterNestedList()
        {
            var provider = LexerBuilder().AddLBracket().AddLBracket().AddRBracket().AddInt().AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            GddlList expected = GddlList.Of(GddlList.Empty(), GddlValue.Of(1));
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void AcceptsTrailingCommaInList()
        {
            ITokenProvider provider = LexerBuilder().AddLBracket().AddInt().AddComma().AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            var expected = GddlList.Of(GddlValue.Of(1));
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesBracesAsMap()
        {
            var provider = LexerBuilder().AddLBrace().AddRBrace().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.AreEqual(GddlMap.Empty(), parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesTypedMap()
        {
            var provider = LexerBuilder().AddIdentifier("test").AddLBrace().AddRBrace().Build();
            Parser parser = Gddl.FromProvider(provider);
            var expected = GddlMap.Empty().WithTypeName("test");
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesNamedValueInsideMap()
        {
            var provider = LexerBuilder().AddLBrace().AddString("\"a\"").AddEquals().AddInt().AddRBrace().Build();
            Parser parser = Gddl.FromProvider(provider);
            GddlMap expected = new GddlMap() { { "a", GddlValue.Of(1) } };
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesNamedTypedNestedMap()
        {
            var provider = LexerBuilder().AddLBrace().AddString("\"n\"").AddEquals().AddIdentifier("a").AddLBrace().AddRBrace().AddRBrace().Build();
            Parser parser = Gddl.FromProvider(provider);
            GddlMap expected = new GddlMap() { { "n", GddlMap.Empty().WithTypeName("a") } };
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesReference()
        {
            var provider = LexerBuilder().AddLBracket().AddIdentifier("a").AddColon().AddIdentifier("b").AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            GddlList expected = GddlList.Of(GddlReference.Of(new Query().ByKey("a").ByKey("b")));
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void ParsesRootedReference()
        {
            var provider = LexerBuilder().AddLBracket().AddColon().AddIdentifier("a").AddColon().AddIdentifier("b").AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            GddlList expected = GddlList.Of(GddlReference.Of(new Query().Absolute().ByKey("a").ByKey("b")));
            Assert.AreEqual(expected, parser.Parse(false).Root);
        }

        [TestMethod]
        public void RejectsMixedDelimiterReference()
        {
            var provider = LexerBuilder().AddLBracket().AddSlash().AddIdentifier("a").AddColon().AddIdentifier("b").AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            Assert.ThrowsException<ParserException>(() => parser.Parse(false));
        }

        [TestMethod]
        public void ParsesSlashReference()
        {
            var provider = LexerBuilder().AddLBracket().AddSlash().AddIdentifier("a").AddSlash().AddIdentifier("b").AddRBracket().Build();
            Parser parser = Gddl.FromProvider(provider);
            GddlList expected = GddlList.Of(GddlReference.Of(new Queries.Query().Absolute().ByKey("a").ByKey("b")));
            Assert.AreEqual(expected, parser.Parse(false).Root);
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
                return Add(TokenType.IntegerLiteral, "1");
            }

            public MockLexerBuilder AddNInt()
            {
                return Add(TokenType.IntegerLiteral, "-1");
            }

            public MockLexerBuilder AddFloat()
            {
                return Add(TokenType.DecimalLiteral, "1.0");
            }

            public MockLexerBuilder AddNFloat()
            {
                return Add(TokenType.DecimalLiteral, "-1.0");
            }

            public MockLexerBuilder AddNaN()
            {
                return Add(TokenType.DecimalLiteral, ".NaN");
            }

            public MockLexerBuilder AddInf()
            {
                return Add(TokenType.DecimalLiteral, ".Inf");
            }

            public MockLexerBuilder AddNInf()
            {
                return Add(TokenType.DecimalLiteral, "-.Inf");
            }

            public MockLexerBuilder AddHexInt()
            {
                return Add(TokenType.HexIntLiteral, "0x1");
            }

            public MockLexerBuilder AddString()
            {
                return Add(TokenType.StringLiteral, "\"1\"");
            }

            public MockLexerBuilder AddString(string text)
            {
                return Add(TokenType.StringLiteral, text);
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
            public MockLexerBuilder AddLBracket()
            {
                return Add(TokenType.LBracket, "[");
            }

            public MockLexerBuilder AddRBracket()
            {
                return Add(TokenType.RBracket, "]");
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

            public MockLexerBuilder AddSlash()
            {
                return Add(TokenType.Slash, "/");
            }

            public MockLexerBuilder AddIdentifier()
            {
                return Add(TokenType.Identifier, "test");
            }

            public MockLexerBuilder AddIdentifier(string text)
            {
                return Add(TokenType.Identifier, text);
            }

            public MockLexerBuilder Add(TokenType name, string text)
            {
                return Add(name, text, new ParsingContext("TEST", 1, 1));
            }
            
            public MockLexerBuilder Add(TokenType name, string text, ParsingContext context, string comment = "", string whitespace = "")
            {
                builder.Add(new Token(name, text, context, comment, whitespace));
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
            public int current = 0;
            public bool closed = false;

            public MockLexer(IReadOnlyList<Token> preparedTokens)
            {
                this.preparedTokens = preparedTokens;
            }

            private Token Get(int index)
            {
                if (closed)
                    throw new InvalidOperationException("The ITokenProvider is closed.");
                int idx = this.current + index;
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

            public Token PeekFull()
            {
                return Get(0);
            }

            public Token Pop()
            {
                Token t = Get(0);
                current++;
                return t;
            }

            public WhitespaceMode WhitespaceMode { get; set; }

            public ParsingContext ParsingContext
            {
                get
                {
                    if (closed)
                        throw new InvalidOperationException("The ITokenProvider is closed.");
                    return preparedTokens[Math.Min(current, preparedTokens.Count - 1)].ParsingContext;
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