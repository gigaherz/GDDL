using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GDDL.Exceptions;
using GDDL.Structure;
using GDDL.Util;

namespace GDDL
{
    public sealed class Parser : IContextProvider, IDisposable
    {
        // Factory Methods

        /**
         * Constructs a Parser instance that reads from the given filename.
         * @param filename The filename to read from.
         * @return A parser ready to process the file.
         */
        public static Parser FromFile(string filename)
        {
            return FromFile(filename, Encoding.UTF8);
        }

        /**
         * Constructs a Parser instance that reads from the given filename.
         * @param filename The filename to read from.
         * @param charset The charset.
         * @return A parser ready to process the file.
         */
        public static Parser FromFile(string filename, Encoding encoding)
        {
            return FromReader(new StreamReader(filename, encoding), filename);
        }

        /**
         * Constructs a Parser instance that reads from the given file.
         * @param file The file to read from.
         * @return A parser ready to process the file.
         */
        public static Parser FromFile(FileInfo file)
        {
            return FromFile(file.FullName);
        }

        /**
         * Constructs a Parser instance that reads from the given file.
         * @param file The file to read from.
         * @param charset The charset.
         * @return A parser ready to process the file.
         */
        public static Parser FromFile(FileInfo file, Encoding encoding)
        {
            return FromFile(file.FullName, encoding);
        }

        /**
         * Constructs a Parser instance that reads from the given string.
         * @param text The text to parse.
         * @return A parser ready to process the file.
         */
        public static Parser FromString(string text, string sourceName = "UNKNOWN")
        {
            return FromReader(new StringReader(text), sourceName);
        }

        /**
         * Constructs a Parser instance that reads from the given reader.
         * @param reader The stream to read from.
         * @return A parser ready to process the file.
         */
        public static Parser FromReader(TextReader text, string sourceName = "UNKNOWN")
        {
            return new Parser(new Lexer(new Reader(text, sourceName)));
        }

        // For unit test purposes
        public static Parser FromProvider(ITokenProvider lexer)
        {
            return new Parser(lexer);
        }

        // Implementation
        int prefixPos = -1;
        readonly Stack<int> prefixStack = new Stack<int>();
        private bool finishedWithRBrace;

        public ITokenProvider Lex { get; }

        private Parser(ITokenProvider lexer)
        {
            Lex = lexer;
        }

        /**
         * Parses the whole file and returns the resulting root element.
         * Equivalent to {@link #parse(boolean)} with simplify=true
         * @return The root element
         */
        public Element Parse()
        {
            return Parse(true);
        }

        /**
         * Parses the whole file and returns the resulting root element.
         * @param simplify If true, the structure
         * @return The root element
         */
        public Element Parse(bool simplify)
        {
            Element ret = Root();

            if (simplify)
            {
                ret.Resolve(ret, null);
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

        private bool PrefixElement()
        {
            return PrefixBasicElement() || PrefixNamedElement();
        }

        private bool PrefixBasicElement()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Nil, TokenType.Null, TokenType.True, TokenType.False,
                TokenType.HexInt, TokenType.Integer, TokenType.Double, TokenType.String);
            EndPrefixScan();

            return r || PrefixReference() || PrefixCollection() || PrefixTypedCollection();
        }

        private bool PrefixNamedElement()
        {
            BeginPrefixScan();
            var r = HasAny(TokenType.Ident, TokenType.String) && HasAny(TokenType.EqualSign);
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

        private Element Root()
        {
            var e = Element();
            PopExpected(TokenType.End);
            return e;
        }

        private Element Element()
        {
            if (PrefixNamedElement()) return NamedElement();
            if (PrefixBasicElement()) return BasicElement();

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
            if (PrefixCollection()) return Set();
            if (PrefixTypedCollection()) return TypedSet();
            if (PrefixReference()) return Reference();

            throw new ParserException(this, "Internal Error");
        }

        private Element NamedElement()
        {
            var name = PopExpected(TokenType.Ident, TokenType.String);

            var n = name.Type == TokenType.Ident ? name.Text : UnescapeString(name);

            PopExpected(TokenType.EqualSign);

            if (!PrefixBasicElement())
                throw new ParserException(this, $"Expected a basic element after EqualSign, found {Lex.Peek()} instead");

            var b = BasicElement();

            b.Name = n;
            b.Comment = name.Comment;

            return b;
        }

        private Reference Reference()
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
            var b = rooted ? Structure.Reference.Absolute(name.Text) : Structure.Reference.Relative(name.Text);
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

        public static Value IntValue(Token token, int @base)
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
            Value v = Value.Of(sign * long.Parse(s[p..],
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
            Value v = Value.Of(UnescapeString(token));
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

        public ParsingContext ParsingContext => Lex.ParsingContext;

        public void Dispose()
        {
            Lex.Dispose();
        }
    }
}
