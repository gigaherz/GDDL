using GDDL.Exceptions;
using GDDL.Structure;
using GDDL.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GDDL.Parsing
{
    public sealed class Parser : IContextProvider, IDisposable
    {
        #region API

        public Parser(ITokenProvider lexer)
        {
            Lexer = lexer;
        }

        private WhitespaceMode WhitespaceMode { get; set; }

        public ITokenProvider Lexer { get; }

        /**
         * Parses the whole file and returns the resulting root element.
         * @param simplify If true, queries are resolved and values are simplified.
         * @return The root element
         */
        public GddlDocument Parse(bool simplify = false)
        {
            var (ret, danglingComment) = Root();

            if (simplify)
            {
                ret.Resolve(ret);
                ret = ret.Simplify();
            }

            var doc = GddlDocument.Create(ret);
            doc.DanglingComment = danglingComment;

            return doc;
        }

        public Queries.Query ParseQuery()
        {
            return QueryPath().path;
        }

        #endregion

        #region Implementation

        int prefixPos = -1;
        readonly Stack<int> prefixStack = new();
        private bool finishedWithRBrace;

        private Token PopExpected(params TokenType[] expected)
        {
            var current = Lexer.Peek();
            if (expected.Any(t => current == t))
                return Lexer.Pop();

            if (expected.Length != 1)
                throw new ParserException(this,
                    $"Unexpected token {current}. Expected one of: {string.Join(", ", expected)}.");

            throw new ParserException(this, $"Unexpected token {current}. Expected: {expected[0]}.");
        }

        private Token PopExpectedWithParent(params TokenType[] expected)
        {
            var current = Lexer.PeekFull();
            if (expected.Any(t => current.Is(t)))
                return Lexer.Pop();

            if (expected.Length != 1)
                throw new ParserException(this,
                    $"Unexpected token {current}. Expected one of: {string.Join(", ", expected)}.");

            throw new ParserException(this, $"Unexpected token {current}. Expected: {expected[0]}.");
        }

        public void BeginPrefixScan()
        {
            prefixStack.Push(prefixPos);
        }

        public TokenType NextPrefix()
        {
            return Lexer.Peek(++prefixPos);
        }

        public void EndPrefixScan()
        {
            prefixPos = prefixStack.Pop();
        }

        private bool HasAny(params TokenType[] tokens)
        {
            var prefix = NextPrefix();
            return tokens.Any(t => prefix == t);
        }

        #endregion

        #region Parse Rules

        private (GddlElement, string) Root()
        {
            var e = Element();
            var end = PopExpected(TokenType.End);
            return (e, end.Comment);
        }

        private bool PrefixIdentifier()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Identifier);
            EndPrefixScan();
            return r;
        }

        private GddlElement Element()
        {
            if (Lexer.Peek() == TokenType.Nil) return NullValue(PopExpected(TokenType.Nil));
            if (Lexer.Peek() == TokenType.Null) return NullValue(PopExpected(TokenType.Null));
            if (Lexer.Peek() == TokenType.True) return BooleanValue(PopExpected(TokenType.True));
            if (Lexer.Peek() == TokenType.False) return BooleanValue(PopExpected(TokenType.False));
            if (Lexer.Peek() == TokenType.IntegerLiteral) return IntValue(PopExpected(TokenType.IntegerLiteral));
            if (Lexer.Peek() == TokenType.HexIntLiteral) return HexIntValue(PopExpected(TokenType.HexIntLiteral));
            if (Lexer.Peek() == TokenType.IntegerLiteral) return IntValue(PopExpected(TokenType.IntegerLiteral));
            if (Lexer.Peek() == TokenType.DecimalLiteral) return FloatValue(PopExpected(TokenType.DecimalLiteral));
            if (Lexer.Peek() == TokenType.StringLiteral) return StringValue(PopExpected(TokenType.StringLiteral));
            if (PrefixMap()) return Map();
            if (PrefixObject()) return Object();
            if (PrefixList()) return List();
            if (PrefixReference()) return Reference();

            throw new ParserException(this,
                $"Internal Error: Token {Lexer.Peek()} did not correspond to any code path.");
        }

        private Token Name()
        {
            return PopExpected(TokenType.Identifier, TokenType.StringLiteral);
        }

        private bool PrefixReference()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Colon, TokenType.Slash) &&
                    HasAny(TokenType.Identifier, TokenType.StringLiteral, TokenType.LBracket);
            EndPrefixScan();

            return r || PrefixIdentifier();
        }

        private GddlReference Reference()
        {
            var (token, path) = QueryPath();

            var b = GddlReference.Of(path);
            b.Comment = token.Comment;

            return b;
        }

        private (Token token, Queries.Query path) QueryPath()
        {
            var path = new Queries.Query();

            Token firstToken = null;

            TokenType? firstDelimiter = null;
            if (Lexer.Peek() == TokenType.Colon || Lexer.Peek() == TokenType.Slash)
            {
                firstToken = PopExpected(TokenType.Colon, TokenType.Slash);
                firstDelimiter = firstToken.Type;
                path = path.Absolute();
            }

            var pathToken = PathComponent(ref path);
            firstToken ??= pathToken;

            while (Lexer.Peek() == TokenType.Colon || Lexer.Peek() == TokenType.Slash)
            {
                if (firstDelimiter.HasValue && Lexer.Peek() != firstDelimiter)
                    throw new ParserException(this,
                        $"Query must use consistent delimiters, expected {firstDelimiter}, found {Lexer.Peek()} instead");

                firstDelimiter = PopExpected(TokenType.Colon, TokenType.Slash).Type;

                PathComponent(ref path);
            }

            return (firstToken, path);
        }

        private Token PathComponent(ref Queries.Query path)
        {
            var token = PopExpected(TokenType.Identifier, TokenType.StringLiteral, TokenType.Dot, TokenType.DoubleDot,
                TokenType.LBracket);
            switch (token.Type)
            {
            case TokenType.Identifier:
                path = path.ByKey(token.Text);
                break;
            case TokenType.StringLiteral:
                path = path.ByKey(UnescapeString(token));
                break;
            case TokenType.Dot:
                path = path.Self();
                break;
            case TokenType.DoubleDot:
                path = path.Parent();
                break;
            case TokenType.LBracket:
            {
                var hasStart = false;
                var start = Index.FromStart(0);

                if (Lexer.Peek() == TokenType.Caret)
                {
                    PopExpected(TokenType.Caret);
                    start = Index.FromEnd((int)IntValue(PopExpected(TokenType.IntegerLiteral)).AsInteger);
                    hasStart = true;
                }
                else if (Lexer.Peek() == TokenType.IntegerLiteral)
                {
                    start = (int)IntValue(PopExpected(TokenType.IntegerLiteral)).AsInteger;
                    hasStart = true;
                }

                if (hasStart && Lexer.Peek() == TokenType.RBracket)
                {
                    PopExpected(TokenType.RBracket);
                    path = path.ByRange(new Range(start, start.Value + 1));
                    break;
                }

                var inclusive = PopExpected(TokenType.DoubleDot, TokenType.TripleDot);

                var end = Index.FromEnd(0);

                if (Lexer.Peek() == TokenType.Caret)
                {
                    PopExpected(TokenType.Caret);
                    end = Index.FromEnd((int)IntValue(PopExpected(TokenType.IntegerLiteral)).AsInteger);
                }
                else if (Lexer.Peek() == TokenType.IntegerLiteral)
                {
                    end = (int)IntValue(PopExpected(TokenType.IntegerLiteral)).AsInteger;
                    if (inclusive.Type == TokenType.TripleDot)
                        end = end.Value + 1;
                }

                PopExpected(TokenType.RBracket);

                path = path.ByRange(new Range(start, end));
                break;
            }
            default:
                throw new ParserException(Lexer,
                    $"Internal Error: Unexpected token {token} found when parsing Reference path component");
            }

            return token;
        }

        private bool PrefixMap()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.LBrace);
            EndPrefixScan();
            return r;
        }

        private GddlMap Map()
        {
            var openBrace = PopExpected(TokenType.LBrace);

            var s = GddlMap.Empty();
            s.Comment = openBrace.Comment;

            while (Lexer.Peek() != TokenType.RBrace)
            {
                finishedWithRBrace = false;

                var name = PopExpectedWithParent(TokenType.Identifier, TokenType.StringLiteral);

                string n = name.Type == TokenType.StringLiteral ? UnescapeString(name) : name.Text;

                PopExpected(TokenType.EqualSign, TokenType.Colon);

                var b = Element();
                b.Comment = name.Comment;
                b.Whitespace = name.Whitespace;
                s.Add(n, b);

                if (Lexer.Peek() != TokenType.RBrace)
                {
                    if (!finishedWithRBrace || Lexer.Peek() == TokenType.Comma)
                    {
                        PopExpected(TokenType.Comma);
                    }
                }
            }

            var end = PopExpected(TokenType.RBrace);
            s.TrailingComment = end.Comment;

            finishedWithRBrace = true;

            return s;
        }

        private bool PrefixObject()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Identifier, TokenType.StringLiteral) && HasAny(TokenType.LBrace);
            EndPrefixScan();
            return r;
        }

        private GddlMap Object()
        {
            var type = Name();

            if (!PrefixMap())
                throw new ParserException(this, "Internal error");

            var s = Map().WithTypeName(type.Text);

            s.Comment = type.Comment;

            return s;
        }

        private bool PrefixList()
        {
            BeginPrefixScan();
            bool r = HasAny(TokenType.LBracket);
            EndPrefixScan();
            return r;
        }

        private GddlList List()
        {
            var openBrace = PopExpected(TokenType.LBracket);

            var s = GddlList.Empty();
            s.Comment = openBrace.Comment;

            while (Lexer.Peek() != TokenType.RBracket)
            {
                finishedWithRBrace = false;

                s.Add(Element());

                if (Lexer.Peek() != TokenType.RBracket)
                {
                    if (!finishedWithRBrace || Lexer.Peek() == TokenType.Comma)
                    {
                        PopExpected(TokenType.Comma);
                    }
                }
            }

            var end = PopExpected(TokenType.RBracket);
            s.TrailingComment = end.Comment;

            finishedWithRBrace = true;

            return s;
        }

        public static GddlValue NullValue(Token token)
        {
            var v = GddlValue.Null();
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue BooleanValue(Token token)
        {
            var v = GddlValue.Of(token.Type == TokenType.True);
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue IntValue(Token token)
        {
            var v = GddlValue.Of(long.Parse(token.Text, CultureInfo.InvariantCulture));
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue HexIntValue(Token token)
        {
            // long.Parse for HexNumber doesn't support sign, but we do.
            // skip the sign if it's present, and apply it later.
            var s = token.Text;
            var p = 2;
            var sign = 1;
            if (s.StartsWith("-"))
            {
                p++;
                sign = -1;
            }

            var num = long.Parse(s[p..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var v = GddlValue.Of(sign * num);
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue FloatValue(Token token)
        {
            var value = token.Text switch
            {
                ".NaN" => double.NaN,
                ".Inf" => double.PositiveInfinity,
                "+.Inf" => double.PositiveInfinity,
                "-.Inf" => double.NegativeInfinity,
                _ => double.Parse(token.Text, CultureInfo.InvariantCulture)
            };
            var v = GddlValue.Of(value);
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue StringValue(Token token)
        {
            var v = GddlValue.Of(UnescapeString(token));
            v.Comment = token.Comment;
            return v;
        }

        public static string UnescapeString(Token t)
        {
            try
            {
                return Utility.UnescapeString(t.Text);
            }
            catch (ArgumentException e)
            {
                throw new ParserException(t, "Unescaping string", e);
            }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"{{Parser lexer={Lexer}}}";
        }

        #endregion

        #region IContextProvider

        public ParsingContext ParsingContext => Lexer.ParsingContext;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Lexer.Dispose();
        }

        #endregion
    }
}