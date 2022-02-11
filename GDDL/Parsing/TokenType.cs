
namespace GDDL.Parsing
{
    public enum TokenType
    {
        Comma,
        EqualSign,
        Colon,
        Slash,
        Dot,
        DoubleDot,
        LBrace,
        RBrace,
        LBracket,
        RBracket,
        Percent,

        HexIntLiteral,
        IntegerLiteral,
        DecimalLiteral,
        StringLiteral,

        Ident,

        // Identifiers
        Nil,
        Null,
        True,
        False,

        Boolean,
        String,
        Integer,
        Decimal,

        // End
        End
    }
}
