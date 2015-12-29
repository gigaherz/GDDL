using System;

namespace GDDL.Exceptions
{
    [Serializable]
    public class ParserException : Exception
    {
        public ParserException(IContextProvider context, string message)
            : base($"{context.GetParsingContext()}: {message}")
        {
        }
    }
}
