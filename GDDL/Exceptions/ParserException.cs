using System;

namespace GDDL.Exceptions
{
    public class ParserException : Exception
    {
        public ParserException(IContextProvider context, string message)
            : base($"{context.ParsingContext}: {message}")
        {
        }
        public ParserException(IContextProvider context, string message, Exception e)
            : base($"{context.ParsingContext}: {message}", e)
        {
        }
    }
}
