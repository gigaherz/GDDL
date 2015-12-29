namespace GDDL
{
    enum Tokens
    {
        NIL,
        NULL,
        TRUE,
        FALSE,
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

    internal class Token
    {
        public Tokens Name { get; private set; }
        public string Text { get; private set; }
        public ParseContext Context { get; private set; }

        public Token(Tokens name, ParseContext context, string text)
        {
            Name = name;
            Text = text;
            Context = context;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Text))
                return string.Format("({0} @ {1}{2})", Name, Context.Line, Context.Column);

            if (Text.Length > 22)
                return string.Format("({0} @ {1}{2}: {3}...)", Name, Context.Line, Context.Column, Text.Substring(20));

            return string.Format("({0} @ {1}{2}: {3})", Name, Context.Line, Context.Column, Text);
        }
    }

}
