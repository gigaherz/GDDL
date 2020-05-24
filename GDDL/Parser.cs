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
        int prefixPos = -1;
        readonly Stack<int> prefixStack = new Stack<int>();
        private bool finishedWithRbrace;

        public Lexer Lexer { get; }

        Parser(Lexer lexer)
        {
            Lexer = lexer;
        }

        public static Parser FromFile(string filename)
        {
            return new Parser(new Lexer(new Reader(new StreamReader(filename), filename)));
        }

        public static Parser FromString(string text)
        {
            return new Parser(new Lexer(new Reader(new StringReader(text), "UNKNOWN")));
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

        private Token PopExpected(params Tokens[] expected)
        {
            Tokens current = Lexer.Peek();
            if (expected.Any(t => current == t))
                return Lexer.Pop();

            if (expected.Length != 1)
                throw new ParserException(this, $"Unexpected token {current}. Expected one of: {string.Join(", ", expected)}.");

            throw new ParserException(this, $"Unexpected token {current}. Expected: {expected[0]}.");
        }

        public void BeginPrefixScan()
        {
            prefixStack.Push(prefixPos);
        }

        public Tokens NextPrefix()
        {
            return Lexer.Peek(++prefixPos);
        }

        public void EndPrefixScan()
        {
            prefixPos = prefixStack.Pop();
        }

        private bool HasAny(params Tokens[] tokens)
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
            var r = HasAny(Tokens.Nil, Tokens.Null, Tokens.True, Tokens.False,
                Tokens.HexInt, Tokens.Integer, Tokens.Double, Tokens.String);
            EndPrefixScan();

            return r || prefix_backreference() || prefix_set() || prefix_typedSet();
        }

        private bool prefix_namedElement()
        {
            BeginPrefixScan();
            var r = HasAny(Tokens.Ident, Tokens.String) && HasAny(Tokens.EqualSign);
            EndPrefixScan();
            return r;
        }

        private bool prefix_backreference()
        {
            BeginPrefixScan();
            var r = HasAny(Tokens.Colon) && HasAny(Tokens.Ident);
            EndPrefixScan();

            return r || prefix_identifier();
        }

        private bool prefix_set()
        {
            BeginPrefixScan();
            var r = HasAny(Tokens.LBrace);
            EndPrefixScan();
            return r;
        }

        private bool prefix_typedSet()
        {
            BeginPrefixScan();
            var r = HasAny(Tokens.Ident) && HasAny(Tokens.LBrace);
            EndPrefixScan();
            return r;
        }

        private bool prefix_identifier()
        {
            BeginPrefixScan();
            var r = HasAny(Tokens.Ident);
            EndPrefixScan();
            return r;
        }

        private Element Root()
        {
            var e = Element();
            PopExpected(Tokens.End);
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
            if (Lexer.Peek() == Tokens.Nil) return NullValue(PopExpected(Tokens.Nil));
            if (Lexer.Peek() == Tokens.Null) return NullValue(PopExpected(Tokens.Null));
            if (Lexer.Peek() == Tokens.True) return BooleanValue(PopExpected(Tokens.True));
            if (Lexer.Peek() == Tokens.False) return BooleanValue(PopExpected(Tokens.False));
            if (Lexer.Peek() == Tokens.Integer) return IntValue(PopExpected(Tokens.Integer));
            if (Lexer.Peek() == Tokens.HexInt) return IntValue(PopExpected(Tokens.HexInt), 16);
            if (Lexer.Peek() == Tokens.Integer) return IntValue(PopExpected(Tokens.Integer));
            if (Lexer.Peek() == Tokens.Double) return FloatValue(PopExpected(Tokens.Double));
            if (Lexer.Peek() == Tokens.String) return StringValue(PopExpected(Tokens.String));
            if (prefix_set()) return Set();
            if (prefix_typedSet()) return TypedSet();
            if (prefix_backreference()) return Backreference();

            throw new ParserException(this, "Internal Error");
        }

        private Element NamedElement()
        {
            var name = PopExpected(Tokens.Ident, Tokens.String);

            var n = name.Name == Tokens.Ident ? name.Text : Lexer.UnescapeString(name);

            PopExpected(Tokens.EqualSign);

            if (!prefix_basicElement())
                throw new ParserException(this, $"Expected a basic element after EqualSign, found {Lexer.Peek()} instead");

            var b = BasicElement();
            b.Comment = name.Comment;

            b.Name = n;

            return b;
        }

        private Reference Backreference()
        {
            var rooted = false;

            if (Lexer.Peek() == Tokens.Colon)
            {
                PopExpected(Tokens.Colon);
                rooted = true;
            }
            if (!prefix_identifier())
                throw new ParserException(this, $"Expected identifier, found {Lexer.Peek()} instead");

            var name = Identifier();
            var b = Structure.Element.Backreference(rooted, name.Text);
            b.Comment = name.Comment;

            while (Lexer.Peek() == Tokens.Colon)
            {
                PopExpected(Tokens.Colon);

                name = Identifier();

                b.Add(name.Text);
            }

            return b;
        }

        private Collection Set()
        {
            var openBrace = PopExpected(Tokens.LBrace);

            var s = Structure.Element.Set();
            s.Comment = openBrace.Comment;

            while (Lexer.Peek() != Tokens.RBrace)
            {
                finishedWithRbrace = false;

                if (!prefix_element())
                    throw new ParserException(this, $"Expected element after LBRACE, found {Lexer.Peek()} instead");

                s.Add(Element());

                if (Lexer.Peek() != Tokens.RBrace)
                {
                    if (!finishedWithRbrace || (Lexer.Peek() == Tokens.Comma))
                    {
                        PopExpected(Tokens.Comma);
                    }
                }
            }

            PopExpected(Tokens.RBrace);

            finishedWithRbrace = true;

            return s;
        }

        private Collection TypedSet()
        {
            var type = Identifier();

            if (!prefix_set())
                throw new ParserException(this, "Internal error");
            var s = Set();

            s.Comment = type.Comment;

            s.TypeName = type.Text;

            return s;
        }

        private Token Identifier()
        {
            if (Lexer.Peek() == Tokens.Ident) return PopExpected(Tokens.Ident);

            throw new ParserException(this, "Internal error");
        }

        public static Value NullValue(Token token)
        {
            Value v = Structure.Element.NullValue();
            v.Comment = token.Comment;
            return v;
        }

        public static Value BooleanValue(Token token)
        {
            Value v = Structure.Element.BooleanValue(token.Name == Tokens.True);
            v.Comment = token.Comment;
            return v;
        }

        public static Value IntValue(Token token)
        {
            Value v = Structure.Element.IntValue(long.Parse(token.Text, CultureInfo.InvariantCulture));
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
            Value v = Structure.Element.IntValue(sign * long.Parse(s.Substring(p), 
                NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            v.Comment = token.Comment;
            return v;
        }

        public static Value FloatValue(Token token)
        {
            Value v = Structure.Element.FloatValue(double.Parse(token.Text, CultureInfo.InvariantCulture));
            v.Comment = token.Comment;
            return v;
        }

        public static Value StringValue(Token token)
        {
            Value v = Structure.Element.StringValue(Lexer.UnescapeString(token));
            v.Comment = token.Comment;
            return v;
        }

        public ParsingContext GetParsingContext()
        {
            return Lexer.GetParsingContext();
        }

        public void Dispose()
        {
            Lexer.Dispose();
        }
    }
}
