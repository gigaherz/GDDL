using GDDL.Parsing;

namespace GDDL.Exceptions
{
    public class ReaderException : LexerException
    {
        public ReaderException(IContextProvider context, string message)
            : base(context, message)
        {
        }
    }
}