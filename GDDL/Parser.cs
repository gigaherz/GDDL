//#define DEBUG_RULES

using GDDL.Structure;
using System.Diagnostics;

namespace GDDL
{
    public class Parser
    {
        public static Parser FromFile(string filename)
        {
            return new Parser(new Lexer(new Reader(filename)));
        }

        readonly Lexer lex;
        internal Parser(Lexer lexer)
        {
            lex = lexer;
        }

        bool finished_with_rbrace = false;

        internal Lexer Lexer { get { return lex; } }

        public Element Parse(bool resolveReferences = true)
        {
            var ret = root();

            if (resolveReferences)
                ret.Resolve(ret);

            return ret;
        }

        private Token pop_expected(Tokens expectedToken)
        {
            if (lex.Peek() != expectedToken)
                throw new ParserException(this, string.Format("Unexpected token {0}: Expected {1}.", lex.Peek(), expectedToken));
            return lex.Pop();
        }

        private bool has_any(params Tokens[] tokens)
        {
            lex.NextPrefix();
            foreach (var t in tokens)
            {
                if (lex.Prefix == t)
                {
                    return true;
                }
            }
            return false;
        }

        private bool has_prefix(params Tokens[] tokens)
        {
            lex.BeginPrefixScan();

            var r = has_any(tokens);

            lex.EndPrefixScan();
            return r;
        }

        Element root()
#if DEBUG_RULES
        {
            Debug.WriteLine("Entering rule_root()");
            var ret = rule_root();
            Debug.WriteLine(string.Format("Finished rule_root(), returned: {0}", ret));
            return ret;
        }
        Element rule_root()
#endif
        {
            var E = element();

            pop_expected(Tokens.END);

            return E;
        }

        bool prefix_element() { return prefix_basicElement() || prefix_namedElement(); }
        Element element()
#if DEBUG_RULES
        {
            Debug.WriteLine("Entering rule_element()");
            var ret = rule_element();
            Debug.WriteLine(string.Format("Finished rule_element(), returned: {0}", ret));
            return ret;
        }
        Element rule_element()
#endif
        {
            if (prefix_namedElement()) return namedElement();
            if (prefix_basicElement()) return basicElement();

            throw new ParserException(this, "Internal Error");
        }

        bool prefix_basicElement()
        {
            return has_prefix(Tokens.NIL, Tokens.NULL, Tokens.TRUE, Tokens.FALSE,
                Tokens.HEXINT, Tokens.INTEGER, Tokens.DOUBLE, Tokens.STRING)
                || prefix_backreference() || prefix_set() || prefix_typedSet();
        }
        Element basicElement()
#if DEBUG_RULES
        {
            Debug.WriteLine("Entering rule_basicElement()");
            var ret = rule_basicElement();
            Debug.WriteLine(string.Format("Finished rule_basicElement(), returned: {0}", ret));
            return ret;
        }
        Element rule_basicElement()
#endif
        {
            if (lex.Peek() == Tokens.NIL) { pop_expected(Tokens.NIL); return Element.Null(); }
            if (lex.Peek() == Tokens.NULL) { pop_expected(Tokens.NULL); return Element.Null(); }
            if (lex.Peek() == Tokens.TRUE) { pop_expected(Tokens.TRUE); return Element.BooleanValue(true); }
            if (lex.Peek() == Tokens.FALSE) { pop_expected(Tokens.FALSE); return Element.BooleanValue(false); }
            if (lex.Peek() == Tokens.INTEGER) return Element.IntValue(pop_expected(Tokens.INTEGER).Text);
            if (lex.Peek() == Tokens.HEXINT) return Element.IntValue(pop_expected(Tokens.HEXINT).Text, 16);
            if (lex.Peek() == Tokens.INTEGER) return Element.IntValue(pop_expected(Tokens.INTEGER).Text);
            if (lex.Peek() == Tokens.DOUBLE) return Element.FloatValue(pop_expected(Tokens.DOUBLE).Text);
            if (lex.Peek() == Tokens.STRING) return Element.StringValue(pop_expected(Tokens.STRING).Text);
            if (prefix_set()) return set();
            if (prefix_typedSet()) return typedSet();
            if (prefix_backreference()) return backreference();

            throw new ParserException(this, "Internal Error");
        }

        bool prefix_namedElement()
        {
            lex.BeginPrefixScan();
            var r = has_any(Tokens.IDENT) && has_any(Tokens.EQUALS);
            lex.EndPrefixScan();
            return r;
        }
        Element namedElement()
#if DEBUG_RULES
        {
            Debug.WriteLine("Entering rule_namedElement()");
            var ret = rule_namedElement();
            Debug.WriteLine(string.Format("Finished rule_namedElement(), returned: {0}", ret));
            return ret;
        }
        NamedElement rule_namedElement()
#endif
        {
            var I = identifier();

            pop_expected(Tokens.EQUALS);

            if (!prefix_basicElement())
                throw new ParserException(this, string.Format("Expected a basic element after EQUALS, found {0} instead", lex.Peek()));

            var B = basicElement();

            B.Name = I;

            return B;
        }

        bool prefix_backreference()
        {
            lex.BeginPrefixScan();
            var r = has_any(Tokens.COLON) && has_any(Tokens.IDENT);
            lex.EndPrefixScan();

            return r || prefix_identifier();
        }
        Backreference backreference()
#if DEBUG_RULES
        {
            Debug.WriteLine("Entering rule_backreference()");
            var ret = rule_backreference();
            Debug.WriteLine(string.Format("Finished rule_backreference(), returned: {0}", ret));
            return ret;
        }
        Backreference rule_backreference()
#endif
        {
            bool rooted = false;

            if (lex.Peek() == Tokens.COLON)
            {
                pop_expected(Tokens.COLON);
                rooted = true;
            }
            if (!prefix_identifier())
                throw new ParserException(this, string.Format("Expected identifier, found {0} instead", lex.Peek()));

            var I = identifier();
            var B = Element.Backreference(rooted, I);

            while (has_prefix(Tokens.COLON))
            {
                pop_expected(Tokens.COLON);

                var O = identifier();

                B.Append(O);
            }

            return B;
        }

        bool prefix_set()
        {
            return has_prefix(Tokens.LBRACE);
        }
        Set set()
#if DEBUG_RULES
        {
            Debug.WriteLine("Entering rule_set()");
            var ret = rule_set();
            Debug.WriteLine(string.Format("Finished rule_set(), returned: {0}", ret));
            return ret;
        }
        Set rule_set()
#endif
        {
            pop_expected(Tokens.LBRACE);

            var S = Element.Set();

            while (lex.Peek() != Tokens.RBRACE)
            {
                finished_with_rbrace = false;

                if (!prefix_element())
                    throw new ParserException(this, string.Format("Expected element after LBRACE, found {0} instead", lex.Peek()));

                S.Append(element());

                if (lex.Peek() != Tokens.RBRACE)
                {
                    if (!finished_with_rbrace || (lex.Peek() == Tokens.COMMA))
                    {
                        pop_expected(Tokens.COMMA);
                    }
                }
            }

            pop_expected(Tokens.RBRACE);

            finished_with_rbrace = true;

            return S;
        }

        bool prefix_typedSet()
        {
            lex.BeginPrefixScan();
            var r = has_any(Tokens.IDENT) && has_any(Tokens.LBRACE);
            lex.EndPrefixScan();
            return r;
        }
        Set typedSet()
#if DEBUG_RULES
        {
            Debug.WriteLine("Entering rule_typedSet()");
            var ret = rule_typedSet();
            Debug.WriteLine(string.Format("Finished rule_typedSet(), returned: {0}", ret));
            return ret;
        }
        TypedSet rule_typedSet()
#endif
        {
            var I = identifier();

            if (!prefix_set())
                throw new ParserException(this, "Internal error");
            var S = set();

            S.Name = I;

            return S;
        }

        bool prefix_identifier()
        {
            return has_prefix(Tokens.IDENT);
        }
        string identifier()
#if DEBUG_RULES
        {
            Debug.WriteLine("Entering rule_identifier()");
            var ret = rule_identifier();
            Debug.WriteLine(string.Format("Finished rule_identifier(), returned: {0}", ret));
            return ret;
        }
        string rule_identifier()
#endif
        {
            if (lex.Peek() == Tokens.IDENT) return pop_expected(Tokens.IDENT).Text;

            throw new ParserException(this, "Internal error");
        }
    }
}
