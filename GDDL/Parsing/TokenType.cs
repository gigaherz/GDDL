
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
        TripleDot,
        LBrace,
        RBrace,
        LBracket,
        RBracket,
        Percent,
        Caret,

        HexIntLiteral,
        IntegerLiteral,
        DecimalLiteral,
        StringLiteral,

        Identifier,

        // Identifiers
        Nil,
        Null,
        True,
        False,

        // Type Identifiers
        Boolean,
        String,
        Integer,
        Decimal,

        // End
        End
    }
}
