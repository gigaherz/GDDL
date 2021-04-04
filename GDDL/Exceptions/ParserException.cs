using System;

namespace GDDL.Exceptions
{
    public class ParserException : Exception
    {
        public ParserException(IContextProvider context, string message)
            : base($"{context.GetParsingContext()}: {message}")
        {
        }
        public ParserException(IContextProvider context, string message, Exception e)
            : base($"{context.GetParsingContext()}: {message}", e)
        {
        }
    }
}
