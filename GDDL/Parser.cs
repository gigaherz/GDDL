using GDDL.Structure;
using System.Collections.Generic;
using System.Text;
using System;
using System.Diagnostics;
using System.IO;

namespace GDDL
{
    enum Token
    {
        COMMA,
        HEXINT,
        INTEGER,
        DOUBLE,
        STRING,
        EQUALS,
        COLON,
        LBRACE,
        RBRACE,
        IDENT,
        END,
        CHAR,
    }

    interface IToken
    {
        Token Name { get; }
        string Text { get; }
        ReaderContext Context { get; }
    }

    interface ILexer
    {
        ReaderContext GetFileContext();

        Token Peek();
        IToken Pop();

        ITokenEnum BeginPrefixScan();
        void EndPrefixScan();
    }

    interface ITokenEnum
    {
        int CurrentPos { get; }
        Token Current { get; }
        void Next();

        ITokenEnum PushRef(ITokenEnum e);
        ITokenEnum PopRef();
    }

    public class Parser
    {
        public static Parser FromFile(string filename)
        {
            return new Parser(new Lexer(new Reader(filename)));
        }

        readonly ILexer lex;
        internal Parser(ILexer lexer)
        {
            lex = lexer;
        }

        bool finished_with_rbrace = false;

        internal ILexer Lexer { get { return lex; } }

        public RootSet Parse()
        {
            return program();
        }

        private IToken pop_expected(Token expectedToken)
        {
            if (lex.Peek() != expectedToken)
                throw new ParseErrorException(this, string.Format("Unexpected token {0}: Expected {1}.", lex.Peek(), expectedToken));
            return lex.Pop();
        }

        private bool has_any(ITokenEnum preview, params Token[] tokens)
        {
            preview.Next();
            foreach (var t in tokens)
            {
                if (preview.Current == t)
                {
                    Debug.WriteLine(string.Format("Matched {0}", preview.Current));
                    return true;
                }
            }
            Debug.WriteLine(string.Format("Looking for one of ({0}), found {1} instead", string.Join(", ", tokens), preview.Current));
            return false;
        }

        private bool has_prefix(params Token[] tokens)
        {
            var preview = lex.BeginPrefixScan();

            var r = has_any(preview, tokens);

            lex.EndPrefixScan();
            return r;
        }

        bool prefix_program() { return prefix_element(); }
        RootSet program()
        {
            Debug.WriteLine("Entering rule_program()");
            var ret = rule_program();
            Debug.WriteLine(string.Format("Finished rule_program(), returned: {0}", ret));
            return ret;
        }
        RootSet rule_program()
        {
            // S=elements { $E = Element.RootSet(S); }

            var S = element();

            var SS = Element.Set();
            SS.Append(S);

            var E = Element.RootSet(SS);

            pop_expected(Token.END);

            return E;
        }

        bool prefix_element() { return prefix_basicElement() || prefix_namedElement(); }
        Element element()
        {
            Debug.WriteLine("Entering rule_element()");
            var ret = rule_element();
            Debug.WriteLine(string.Format("Finished rule_element(), returned: {0}", ret));
            return ret;
        }
        Element rule_element()
        {
            // B=basicElement   { $E = B; } | N=namedElement   { $E = N; }

            if (prefix_namedElement()) return namedElement();
            if (prefix_basicElement()) return basicElement();

            throw new ParseErrorException(this);
        }

        bool prefix_basicElement()
        {
            return has_prefix(Token.HEXINT, Token.INTEGER, Token.DOUBLE, Token.STRING)
                || prefix_backreference() || prefix_set() || prefix_namedSet();
        }
        Element basicElement()
        {
            Debug.WriteLine("Entering rule_basicElement()");
            var ret = rule_basicElement();
            Debug.WriteLine(string.Format("Finished rule_basicElement(), returned: {0}", ret));
            return ret;
        }
        Element rule_basicElement()
        {
            //   H=HEXINT  { $E = Element.IntValue(H.Text,16); }
            // | I=INTEGER  { $E = Element.IntValue(I.Text); }
            // | D=DOUBLE  { $E = Element.FloatValue(D.Text); }
            // | S=STRING  { $E = Element.StringValue(S.Text); }
            // | B=backreference { $E = B; }
            // | T=set  { $E = T; }
            // | N=namedSet  { $E = N; }

            if (lex.Peek() == Token.HEXINT) return Element.IntValue(pop_expected(Token.HEXINT).Text, 16);
            if (lex.Peek() == Token.INTEGER) return Element.IntValue(pop_expected(Token.INTEGER).Text);
            if (lex.Peek() == Token.DOUBLE) return Element.FloatValue(pop_expected(Token.DOUBLE).Text);
            if (lex.Peek() == Token.STRING) return Element.StringValue(pop_expected(Token.STRING).Text);
            if (prefix_set()) return set();
            if (prefix_namedSet()) return namedSet();
            if (prefix_backreference()) return backreference();

            throw new ParseErrorException(this);
        }

        bool prefix_namedElement()
        {
            var p = lex.BeginPrefixScan();
            var r = has_any(p, Token.IDENT) && has_any(p, Token.EQUALS);
            lex.EndPrefixScan();
            return r;
        }
        NamedElement namedElement()
        {
            Debug.WriteLine("Entering rule_namedElement()");
            var ret = rule_namedElement();
            Debug.WriteLine(string.Format("Finished rule_namedElement(), returned: {0}", ret));
            return ret;
        }
        NamedElement rule_namedElement()
        {
            // I=identifier EQUALS B=basicElement  { $N = Element.NamedElement(I, B); }

            var I = identifier();

            pop_expected(Token.EQUALS);

            if (!prefix_basicElement())
                throw new ParseErrorException(this);

            var B = basicElement();

            return Element.NamedElement(I, B);
        }

        bool prefix_backreference()
        {
            var preview = lex.BeginPrefixScan();
            var r = has_any(preview, Token.COLON) && has_any(preview, Token.IDENT);
            lex.EndPrefixScan();

            return r || prefix_identifier();
        }
        Backreference backreference()
        {
            Debug.WriteLine("Entering rule_backreference()");
            var ret = rule_backreference();
            Debug.WriteLine(string.Format("Finished rule_backreference(), returned: {0}", ret));
            return ret;
        }
        Backreference rule_backreference()
        {
            bool rooted = false;

            //  (COLON { rooted = true; })?
            if (lex.Peek() == Token.COLON)
            {
                pop_expected(Token.COLON);
                rooted = true;
            }

            //(I=identifier { $B = Element.Backreference(rooted, I); })

            if (!prefix_identifier())
                throw new ParseErrorException(this);

            var I = identifier();
            var B = Element.Backreference(rooted, I);

            // (COLON I=identifier { $B.Append(I); })*
            while (has_prefix(Token.COLON))
            {
                pop_expected(Token.COLON);

                var O = identifier();

                B.Append(O);
            }

            return B;
        }

        bool prefix_set()
        {
            return has_prefix(Token.LBRACE);
        }
        Set set()
        {
            Debug.WriteLine("Entering rule_set()");
            var ret = rule_set();
            Debug.WriteLine(string.Format("Finished rule_set(), returned: {0}", ret));
            return ret;
        }
        Set rule_set()
        {
            //   LBRACE E=elements RBRACE  { $S = E; }
            // | LBRACE RBRACE  { $S = Element.Set(); }

            pop_expected(Token.LBRACE);

            var S = Element.Set();

            while (lex.Peek() != Token.RBRACE)
            {
                finished_with_rbrace = false;

                if (!prefix_element())
                    throw new ParseErrorException(this);

                S.Append(element());

                if (lex.Peek() != Token.RBRACE)
                {
                    if (!finished_with_rbrace || (lex.Peek() == Token.COMMA))
                    {
                        pop_expected(Token.COMMA);
                    }
                }
            }

            pop_expected(Token.RBRACE);

            finished_with_rbrace = true;

            return S;
        }

        bool prefix_namedSet()
        {
            var p = lex.BeginPrefixScan();
            var r = has_any(p, Token.IDENT) && has_any(p, Token.LBRACE);
            lex.EndPrefixScan();
            return r;
        }
        TypedSet namedSet()
        {
            Debug.WriteLine("Entering rule_namedSet()");
            var ret = rule_namedSet();
            Debug.WriteLine(string.Format("Finished rule_namedSet(), returned: {0}", ret));
            return ret;
        }
        TypedSet rule_namedSet()
        {
            // I=identifier S=set  { $N = Element.TypedSet(I, S); }
            var I = identifier();

            if (!prefix_set())
                throw new ParseErrorException(this);
            var S = set();

            return Element.TypedSet(I, S);
        }

        bool prefix_identifier()
        {
            return has_prefix(Token.IDENT);
        }
        string identifier()
        {
            Debug.WriteLine("Entering rule_identifier()");
            var ret = rule_identifier();
            Debug.WriteLine(string.Format("Finished rule_identifier(), returned: {0}", ret));
            return ret;
        }
        string rule_identifier()
        {
            if (lex.Peek() == Token.IDENT) return pop_expected(Token.IDENT).Text;

            throw new ParseErrorException(this);
        }
    }

    public class ReaderContext
    {
        public string Filename;
        public int Line;
        public int Column;

        public ReaderContext(string f, int l, int c)
        {
            Filename = f;
            Line = l;
            Column = c;
        }

        public override string ToString()
        {
            return string.Format("{0}({1},{2})", Filename, Line, Column);
        }
    }

    class Reader
    {
        bool endQueued = false;
        readonly Deque<int> unreadBuffer = new Deque<int>();

        TextReader dataSource;
        string sourceName;
        int line = 1;
        int column = 1;

        int lastEol;

        public Reader(string source)
        {
            sourceName = source;
            dataSource = new StreamReader(source);
        }

        void Require(int number)
        {
            int needed = number - unreadBuffer.Count;
            if (needed > 0)
            {
                NeedChars(needed);
            }
        }

        private void NeedChars(int needed)
        {
            while (needed-- > 0)
            {
                if (endQueued)
                {
                    throw new ReaderErrorException(this);
                }

                int ch = dataSource.Read();
                unreadBuffer.AddBack(ch);
                if (ch < 0)
                    endQueued = true;
            }
        }

        public int Peek()
        {
            return Peek(0);
        }

        public int Peek(int index)
        {
            Require(index + 1);

            return unreadBuffer[index];
        }

        public int Pop()
        {
            int ch = unreadBuffer.RemoveFront();

            column++;
            if (ch == '\n')
            {
                if (lastEol != '\r')
                {
                    column = 1;
                    line++;
                }
                lastEol = ch;
            }
            else if (ch == '\r')
            {
                lastEol = ch;
            }
            else if (lastEol > 0)
            {
                lastEol = 0;
                column = 1;
                line++;
            }

            return ch;
        }

        public string Read(int count)
        {
            Require(count);
            StringBuilder b = new StringBuilder();
            while (count-- > 0)
            {
                var ch = Pop();
                if (ch < 0)
                    throw new ReaderErrorException(this);
                b.Append((char)ch);
            }
            return b.ToString();
        }

        public void Drop(int count)
        {
            Require(count);
            while (count-- > 0)
                Pop();
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (var ch in unreadBuffer)
            {
                b.Append((char)ch);
            }
            return string.Format("{{Reader ahead={0}}}", b.ToString());
        }

        internal ReaderContext GetFileContext()
        {
            return new ReaderContext(sourceName, line, column);
        }
    }

    class Lexer : ILexer
    {
        readonly Deque<IToken> lookAhead = new Deque<IToken>();

        bool seenEnd = false;

        Reader reader;
        public Lexer(Reader r)
        {
            reader = r;
        }

        class PrefixScanner : ITokenEnum
        {
            Lexer parent;
            int currentPos = 0;
            IToken current;
            ITokenEnum previous;

            public int CurrentPos { get { return currentPos; } }
            public Token Current { get { return current.Name; } }

            internal PrefixScanner(Lexer l)
            {
                parent = l;
            }

            public void Next()
            {
                parent.Require(currentPos + 1);

                current = parent.lookAhead[currentPos++];
            }

            public void EndPreview()
            {
            }

            public override string ToString()
            {
                return string.Format("{0}]", current);
            }

            public ITokenEnum PushRef(ITokenEnum previous)
            {
                this.previous = previous;
                if (previous != null)
                    currentPos = previous.CurrentPos;
                return this;
            }

            public ITokenEnum PopRef()
            {
                return previous;
            }
        }

        private void Require(int count)
        {
            int needed = count - lookAhead.Count;
            if (needed > 0)
            {
                ReadAhead(needed);
            }
        }

        ITokenEnum preview = null;
        public ITokenEnum BeginPrefixScan()
        {
            preview = new PrefixScanner(this).PushRef(preview);
            return preview;
        }

        public void EndPrefixScan()
        {
            preview = preview.PopRef();
        }

        public Token Peek()
        {
            Require(1);

            return lookAhead[0].Name;
        }

        public IToken Pop()
        {
            Require(2);

            var t = lookAhead.RemoveFront();

            Debug.WriteLine(string.Format("Popped {0}. Next up: {1}", t, Peek()));

            return t;
        }

        private void ReadAhead(int needed)
        {
            while (needed-- > 0)
            {
                var t = ParseOne();

                lookAhead.AddBack(t);
            }
        }

        private IToken ParseOne()
        {
            char ch;

            if (seenEnd)
                return new SimpleToken(Token.END, reader.GetFileContext(), "");

            int ich = reader.Peek();
            while (true)
            {
                if (ich < 0) return new SimpleToken(Token.END, reader.GetFileContext(), "");

                ch = (char)ich;
                switch (ch)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        reader.Drop(1);

                        ich = reader.Peek();
                        break;
                    case '#':
                        // comment, skip until \r or \n
                        // COMMENT : '#' ( options {greedy=false;} : . )* ('\n' | '\r') {$channel=Hidden;}
                        do
                        {
                            reader.Drop(1);

                            ich = reader.Peek();
                        }
                        while (ich > 0 && ich != '\n' && ich != '\r');
                        break;
                    default:
                        goto blah;
                }
            }

            blah:
            ch = (char)ich;
            switch (ch)
            {
                case '{': return new SimpleToken(Token.LBRACE, reader.GetFileContext(), reader.Read(1));
                case '}': return new SimpleToken(Token.RBRACE, reader.GetFileContext(), reader.Read(1));
                case ',': return new SimpleToken(Token.COMMA, reader.GetFileContext(), reader.Read(1));
                case ':': return new SimpleToken(Token.COLON, reader.GetFileContext(), reader.Read(1));
                case '=': return new SimpleToken(Token.EQUALS, reader.GetFileContext(), reader.Read(1));
            }

            if (char.IsLetter(ch) || ch == '_')
            {
                // IDENT : ('a'..'z'|'A'..'Z'|'_'|'-') ('a'..'z'|'A'..'Z'|'0'..'9'|'_'|'-')*
                int number = 1;
                while (true)
                {
                    ich = reader.Peek(number);
                    if (ich < 0)
                        break;

                    ch = (char)ich;
                    if (char.IsLetter(ch) || char.IsLetterOrDigit(ch) || ch == '_')
                    {
                        number++;
                    }
                    else
                    {
                        break;
                    }
                }

                return new SimpleToken(Token.IDENT, reader.GetFileContext(), reader.Read(number));
            }

            if (ch == '\'')
            {
                //CHAR : '\'' ( ESC_SEQ | ~('\''|'\\') ) '\''
                int number = 1;

                ich = reader.Peek(number);

                if (reader.Peek(number) == '\\')
                {
                    number = count_escape_seq(number);
                }
                else
                {
                    if (ich == '\r')
                    {
                        throw new LexerErrorException(this, string.Format("Expected '\\r', found {0}", (char)ich));
                    }
                    number++;
                }

                ich = reader.Peek(number);
                if (ich != '\'')
                {
                    throw new LexerErrorException(this, string.Format("Expected '\\'', found {0}", (char)ich));
                }

                number++;

                return new SimpleToken(Token.CHAR, reader.GetFileContext(), reader.Read(number));
            }

            if (ch == '"')
            {
                //STRING : '"' ( ESC_SEQ | ~('\\'|'"') )* '"'
                int number = 1;

                ich = reader.Peek(number);
                while (ich != '"')
                {
                    if (reader.Peek(number) == '\\')
                    {
                        number = count_escape_seq(number);
                    }
                    else
                    {
                        if (ich == '\r')
                        {
                            throw new LexerErrorException(this, string.Format("Expected '\\r', found {0}", (char)ich));
                        }
                        number++;
                    }

                    ich = reader.Peek(number);
                }

                if (ich != '"')
                {
                    throw new LexerErrorException(this, string.Format("Expected '\"', found {0}", (char)ich));
                }

                number++;

                return new SimpleToken(Token.STRING, reader.GetFileContext(), reader.Read(number));
            }

            if (char.IsDigit(ch) || ch == '.')
            {
                // numbers
                int number = 0;
                bool fractional = false;

                if (char.IsDigit(ch))
                {
                    if (reader.Peek(0) == '0' && reader.Peek(1) == 'x')
                    {
                        //HEXINT : '0x' HEX_DIGIT+ 
                        //fragment
                        //HEX_DIGIT : ('0'..'9'|'a'..'f'|'A'..'F') ;
                        //

                        number = 2;

                        ich = reader.Peek(number);
                        while (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                        {
                            number++;

                            ich = reader.Peek(number);
                        }

                        return new SimpleToken(Token.HEXINT, reader.GetFileContext(), reader.Read(number));
                    }

                    //INTEGER : '0'..'9'+

                    number = 1;
                    ich = reader.Peek(number);
                    while (char.IsNumber((char)ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                if (ch == '.')
                {
                    fractional = true;
                    // fragment
                    // DECIMAL : '.' ('0'..'9')+

                    // skip the '.'
                    number++;

                    ich = reader.Peek(number);
                    if (!char.IsDigit((char)ich))
                        throw new LexerErrorException(this, string.Format("Expected DIGIT, found {0}", (char)ich));

                    while (char.IsNumber((char)ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                if (ch == 'e' || ch == 'E')
                {
                    fractional = true;
                    //fragment
                    //EXPONENT : ('e'|'E') ('+'|'-')? ('0'..'9')+ ;

                    // letter
                    number++;

                    ich = reader.Peek(number);
                    if (ich == '+' || ich == '-')
                    {
                        number++;

                        ich = reader.Peek(number);
                    }

                    if (!char.IsDigit((char)ich))
                        throw new LexerErrorException(this, string.Format("Expected DIGIT, found {0}", (char)ich));

                    while (char.IsNumber((char)ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                if (fractional)
                    return new SimpleToken(Token.DOUBLE, reader.GetFileContext(), reader.Read(number));

                return new SimpleToken(Token.INTEGER, reader.GetFileContext(), reader.Read(number));
            }

            throw new LexerErrorException(this, string.Format("Unexpected character: {0}", reader.Peek()));
        }

        private int count_escape_seq(int number)
        {
            int ich = reader.Peek(number);
            if (ich != '\\')
                throw new LexerErrorException(this);

            number++;

            //fragment
            //ESC_SEQ : '\\' ('b'|'t'|'n'|'f'|'r'|'\"'|'\''|'\\')

            ich = reader.Peek(number);
            switch (ich)
            {
                case 'b':
                case 'f':
                case 'n':
                case 'r':
                case 't':
                case '"':
                case '\'':
                case '\\':
                    return ++number;
            }

            //fragment
            //UNICODE_ESC :'\\' 'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT

            if (ich == 'x')
            {
                number++;

                if (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                {
                    number++;

                    if (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                    {
                        number++;

                        if (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                        {
                            number++;

                            if (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                            {
                                number++;
                            }
                        }
                    }
                }
                return number;
            }

            throw new LexerErrorException(this, string.Format("Unknown escape sequence \\{0}", ich));
        }

        public override string ToString()
        {
            return string.Format("{{Lexer ahead={0}, reader={1}}}", string.Join(", ", lookAhead), reader);
        }

        public ReaderContext GetFileContext()
        {
            Require(1);
            return lookAhead[0].Context;
        }
    }

    internal class SimpleToken : IToken
    {
        private Token name;
        private string text;
        private ReaderContext context;

        public SimpleToken(Token name, ReaderContext context, string text)
        {
            this.name = name;
            this.text = text;
            this.context = context;
        }

        public Token Name
        {
            get
            {
                return name;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
        }

        public ReaderContext Context
        {
            get
            {
                return context;
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(text))
                return string.Format("({0} @ {1}{2})", Name, context.Line, context.Column);

            if (text.Length > 22)
                return string.Format("({0} @ {1}{2}: {3}...)", Name, context.Line, context.Column, text.Substring(20));

            return string.Format("({0} @ {1}{2}: {3})", Name, context.Line, context.Column, text);
        }
    }
}