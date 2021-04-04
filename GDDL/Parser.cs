using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GDDL.Exceptions;
using GDDL.Structure;

namespace GDDL
{
    public sealed class Parser : IContextProvider, IDisposable
    {
        // Factory Methods
        public static Parser FromFile(string filename)
        {
            return new Parser(new Lexer(new Reader(new StreamReader(filename), filename)));
        }

        public static Parser FromString(string text)
        {
            return new Parser(new Lexer(new Reader(new StringReader(text), "UNKNOWN")));
        }

        // Implementation
        int prefixPos = -1;
        readonly Stack<int> prefixStack = new Stack<int>();
        private bool finishedWithRbrace;

        public ITokenProvider Lex { get; }

        public Parser(ITokenProvider lexer)
        {
            Lex = lexer;
        }

        public Element Parse()
        {
            return Parse(true);
        }

        public Element Parse(bool simplify)
        {
            Element ret = Root();

            if (simplify)
            {
                ret.Resolve(ret, ret);
                ret = ret.Simplify();
            }

            return ret;
        }

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

        private bool prefix_element()
        {
            return prefix_basicElement() || prefix_namedElement();
        }

        private bool prefix_basicElement()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Nil, TokenType.Null, TokenType.True, TokenType.False,
                TokenType.HexInt, TokenType.Integer, TokenType.Double, TokenType.String);
            EndPrefixScan();

            return r || prefix_backreference() || prefix_set() || prefix_typedSet();
        }

        private bool prefix_namedElement()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Ident, TokenType.String) && HasAny(TokenType.EqualSign);
            EndPrefixScan();
            return r;
        }

        private bool prefix_backreference()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Colon) && HasAny(TokenType.Ident);
            EndPrefixScan();

            return r || prefix_identifier();
        }

        private bool prefix_set()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.LBrace);
            EndPrefixScan();
            return r;
        }

        private bool prefix_typedSet()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Ident) && HasAny(TokenType.LBrace);
            EndPrefixScan();
            return r;
        }

        private bool prefix_identifier()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Ident);
            EndPrefixScan();
            return r;
        }

        private Element Root()
        {
            var e = Element();
            PopExpected(TokenType.End);
            return e;
        }

        private Element Element()
        {
            if (prefix_namedElement()) return NamedElement();
            if (prefix_basicElement()) return BasicElement();

            throw new ParserException(this, "Internal Error");
        }

        private Element BasicElement()
        {
            if (Lex.Peek() == TokenType.Nil) return NullValue(PopExpected(TokenType.Nil));
            if (Lex.Peek() == TokenType.Null) return NullValue(PopExpected(TokenType.Null));
            if (Lex.Peek() == TokenType.True) return BooleanValue(PopExpected(TokenType.True));
            if (Lex.Peek() == TokenType.False) return BooleanValue(PopExpected(TokenType.False));
            if (Lex.Peek() == TokenType.Integer) return IntValue(PopExpected(TokenType.Integer));
            if (Lex.Peek() == TokenType.HexInt) return IntValue(PopExpected(TokenType.HexInt), 16);
            if (Lex.Peek() == TokenType.Integer) return IntValue(PopExpected(TokenType.Integer));
            if (Lex.Peek() == TokenType.Double) return FloatValue(PopExpected(TokenType.Double));
            if (Lex.Peek() == TokenType.String) return StringValue(PopExpected(TokenType.String));
            if (prefix_set()) return Set();
            if (prefix_typedSet()) return TypedSet();
            if (prefix_backreference()) return Backreference();

            throw new ParserException(this, "Internal Error");
        }

        private Element NamedElement()
        {
            var name = PopExpected(TokenType.Ident, TokenType.String);

            var n = name.Type == TokenType.Ident ? name.Text : Lexer.UnescapeString(name);

            PopExpected(TokenType.EqualSign);

            if (!prefix_basicElement())
                throw new ParserException(this, $"Expected a basic element after EqualSign, found {Lex.Peek()} instead");

            var b = BasicElement();

            b.Name = n;
            b.Comment = name.Comment;

            return b;
        }

        private Reference Backreference()
        {
            var rooted = false;

            if (Lex.Peek() == TokenType.Colon)
            {
                PopExpected(TokenType.Colon);
                rooted = true;
            }
            if (!prefix_identifier())
                throw new ParserException(this, $"Expected identifier, found {Lex.Peek()} instead");

            var name = Identifier();
            var b = rooted ? Reference.Absolute(name.Text) : Reference.Relative(name.Text);
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
                finishedWithRbrace = false;

                if (!prefix_element())
                    throw new ParserException(this, $"Expected element after LBRACE, found {Lex.Peek()} instead");

                s.Add(Element());

                if (Lex.Peek() != TokenType.RBrace)
                {
                    if (!finishedWithRbrace || (Lex.Peek() == TokenType.Comma))
                    {
                        PopExpected(TokenType.Comma);
                    }
                }
            }

            PopExpected(TokenType.RBrace);

            finishedWithRbrace = true;

            return s;
        }

        private Collection TypedSet()
        {
            var type = Identifier();

            if (!prefix_set())
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

        public static Value NullValue(Token token)
        {
            Value v = Value.Null();
            v.Comment = token.Comment;
            return v;
        }

        public static Value BooleanValue(Token token)
        {
            Value v = Value.Of(token.Type == TokenType.True);
            v.Comment = token.Comment;
            return v;
        }

        public static Value IntValue(Token token)
        {
            Value v = Value.Of(long.Parse(token.Text, CultureInfo.InvariantCulture));
            v.Comment = token.Comment;
            return v;
        }

        public static Value IntValue(Token token, int _base)
        {
            var s = token.Text;
            var p = 2;
            var sign = 1;
            if (s.StartsWith("-"))
            {
                p++;
                sign = -1;
            }
            Value v = Value.Of(sign * long.Parse(s.Substring(p),
                NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            v.Comment = token.Comment;
            return v;
        }

        public static Value FloatValue(Token token)
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
            Value v = Value.Of(value);
            v.Comment = token.Comment;
            return v;
        }

        public static Value StringValue(Token token)
        {
            Value v = Value.Of(Lexer.UnescapeString(token));
            v.Comment = token.Comment;
            return v;
        }

        public ParsingContext GetParsingContext()
        {
            return Lex.GetParsingContext();
        }

        public void Dispose()
        {
            Lex.Dispose();
        }
    }
}
