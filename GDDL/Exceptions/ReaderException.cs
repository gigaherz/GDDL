using GDDL.Parsing;

namespace GDDL.Exceptions
{
    public class ReaderException(IContextProvider context, string message) : LexerException(context, message)
    {
    }
}