using System;

namespace GDDL.Exceptions
{
    public class LexerException : ParserException
    {
        public LexerException(IContextProvider context, string message)
            : base(context, message)
        {
        }
    }
}
