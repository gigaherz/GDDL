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
            Lex = lexer;
        }

        private WhitespaceMode WhitespaceMode { get; set; }

        public ITokenProvider Lex { get; }
        
        /**
         * Parses the whole file and returns the resulting root element.
         * @param simplify If true, the structure
         * @return The root element
         */
        public GddlDocument Parse(bool simplify = true)
        {
            var (ret, danglingComment) = Root();

            if (simplify)
            {
                ret.Resolve(ret, null);
                ret = ret.Simplify();
            }

            var doc = new GddlDocument(root);
            doc.setDanglingComment(result.getValue());

            return doc;
        }
        #endregion


        #region Implementation
        int prefixPos = -1;
        readonly Stack<int> prefixStack = new Stack<int>();
        private bool finishedWithRBrace;

        private Token PopExpected(params TokenType[] expected)
        {
            TokenType current = Lex.Peek();
            if (expected.Any(t => current == t))
                return Lex.Pop();

            if (expected.Length != 1)
                throw new ParserException(this, $"Unexpected token {current}. Expected one of: {string.Join(", ", expected)}.");

            throw new ParserException(this, $"Unexpected token {current}. Expected: {expected[0]}.");
        }

        public void BeginPrefixScan()
        {
            prefixStack.Push(prefixPos);
        }

        public TokenType NextPrefix()
        {
            return Lex.Peek(++prefixPos);
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

        private bool PrefixElement()
        {
            return PrefixBasicElement() || PrefixNamedElement();
        }

        private bool PrefixBasicElement()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Nil, TokenType.Null, TokenType.True, TokenType.False,
                TokenType.HexIntLiteral, TokenType.IntegerLiteral, TokenType.DecimalLiteral, TokenType.StringLiteral);
            EndPrefixScan();

            return r || PrefixReference() || PrefixCollection() || PrefixTypedCollection();
        }

        private bool PrefixNamedElement()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Ident, TokenType.StringLiteral) && HasAny(TokenType.EqualSign);
            EndPrefixScan();
            return r;
        }

        private bool PrefixReference()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Colon) && HasAny(TokenType.Ident);
            EndPrefixScan();

            return r || PrefixIdentifier();
        }

        private bool PrefixCollection()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.LBrace);
            EndPrefixScan();
            return r;
        }

        private bool PrefixTypedCollection()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Ident) && HasAny(TokenType.LBrace);
            EndPrefixScan();
            return r;
        }

        private bool PrefixIdentifier()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Ident);
            EndPrefixScan();
            return r;
        }

        private (GddlElement, string) Root()
        {
            var e = Element();
            var end = PopExpected(TokenType.End);
            return (e, end.Comment);
        }

        private GddlElement Element()
        {
            if (PrefixNamedElement()) return NamedElement();
            if (PrefixBasicElement()) return BasicElement();

            throw new ParserException(this, "Internal Error");
        }

        private GddlElement BasicElement()
        {
            if (Lex.Peek() == TokenType.Nil) return NullValue(PopExpected(TokenType.Nil));
            if (Lex.Peek() == TokenType.Null) return NullValue(PopExpected(TokenType.Null));
            if (Lex.Peek() == TokenType.True) return BooleanValue(PopExpected(TokenType.True));
            if (Lex.Peek() == TokenType.False) return BooleanValue(PopExpected(TokenType.False));
            if (Lex.Peek() == TokenType.IntegerLiteral) return IntValue(PopExpected(TokenType.IntegerLiteral));
            if (Lex.Peek() == TokenType.HexIntLiteral) return IntValue(PopExpected(TokenType.HexIntLiteral), 16);
            if (Lex.Peek() == TokenType.IntegerLiteral) return IntValue(PopExpected(TokenType.IntegerLiteral));
            if (Lex.Peek() == TokenType.DecimalLiteral) return FloatValue(PopExpected(TokenType.DecimalLiteral));
            if (Lex.Peek() == TokenType.StringLiteral) return StringValue(PopExpected(TokenType.StringLiteral));
            if (PrefixCollection()) return Set();
            if (PrefixTypedCollection()) return TypedSet();
            if (PrefixReference()) return Reference();

            throw new ParserException(this, "Internal Error");
        }

        private GddlElement NamedElement()
        {
            var name = PopExpected(TokenType.Ident, TokenType.StringLiteral);

            var n = name.Type == TokenType.Ident ? name.Text : UnescapeString(name);

            PopExpected(TokenType.EqualSign);

            if (!PrefixBasicElement())
                throw new ParserException(this, $"Expected a basic element after EqualSign, found {Lex.Peek()} instead");

            var b = BasicElement();

            b.Name = n;
            b.Comment = name.Comment;

            return b;
        }

        private GddlReference Reference()
        {
            var rooted = false;

            if (Lex.Peek() == TokenType.Colon)
            {
                PopExpected(TokenType.Colon);
                rooted = true;
            }
            if (!PrefixIdentifier())
                throw new ParserException(this, $"Expected identifier, found {Lex.Peek()} instead");

            var name = Identifier();
            var b = rooted ? Structure.GddlReference.Absolute(name.Text) : Structure.GddlReference.Relative(name.Text);
            b.Comment = name.Comment;

            while (Lex.Peek() == TokenType.Colon)
            {
                PopExpected(TokenType.Colon);

                name = Identifier();

                b.Add(name.Text);
            }

            return b;
        }

        private Collection Set()
        {
            var openBrace = PopExpected(TokenType.LBrace);

            var s = Collection.Empty();
            s.Comment = openBrace.Comment;

            while (Lex.Peek() != TokenType.RBrace)
            {
                finishedWithRBrace = false;

                if (!PrefixElement())
                    throw new ParserException(this, $"Expected element after LBRACE, found {Lex.Peek()} instead");

                s.Add(Element());

                if (Lex.Peek() != TokenType.RBrace)
                {
                    if (!finishedWithRBrace || (Lex.Peek() == TokenType.Comma))
                    {
                        PopExpected(TokenType.Comma);
                    }
                }
            }

            PopExpected(TokenType.RBrace);

            finishedWithRBrace = true;

            return s;
        }

        private Collection TypedSet()
        {
            var type = Identifier();

            if (!PrefixCollection())
                throw new ParserException(this, "Internal error");
            var s = Set();
            s.TypeName = type.Text;

            s.Comment = type.Comment;

            return s;
        }

        private Token Identifier()
        {
            if (Lex.Peek() == TokenType.Ident) return PopExpected(TokenType.Ident);

            throw new ParserException(this, "Internal error");
        }

        public static GddlValue NullValue(Token token)
        {
            GddlValue v = GddlValue.Null();
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue BooleanValue(Token token)
        {
            GddlValue v = GddlValue.Of(token.Type == TokenType.True);
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue IntValue(Token token)
        {
            GddlValue v = GddlValue.Of(long.Parse(token.Text, CultureInfo.InvariantCulture));
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue IntValue(Token token, int @base)
        {
            if (@base != 16)
                throw new NotImplementedException("IntValue is only implemented for base=16");
            var s = token.Text;
            var p = 2;
            var sign = 1;
            if (s.StartsWith("-"))
            {
                p++;
                sign = -1;
            }
            GddlValue v = GddlValue.Of(sign * long.Parse(s[p..],
                NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue FloatValue(Token token)
        {
            double value;
            switch (token.Text)
            {
                case ".NaN":
                    value = double.NaN;
                    break;
                case ".Inf":
                case "+.Inf":
                    value = double.PositiveInfinity;
                    break;
                case "-.Inf":
                    value = double.NegativeInfinity;
                    break;
                default:
                    value = double.Parse(token.Text, CultureInfo.InvariantCulture);
                    break;
            }
            GddlValue v = GddlValue.Of(value);
            v.Comment = token.Comment;
            return v;
        }

        public static GddlValue StringValue(Token token)
        {
            GddlValue v = GddlValue.Of(UnescapeString(token));
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

        #region IContextProvider
        public ParsingContext ParsingContext => Lex.ParsingContext;
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Lex.Dispose();
        }
        #endregion
    }
}
