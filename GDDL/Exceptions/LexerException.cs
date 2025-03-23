using GDDL.Parsing;

namespace GDDL.Exceptions
{
    public class LexerException(IContextProvider context, string message) : ParserException(context, message)
    {
    }
}