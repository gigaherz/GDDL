using System;

namespace GDDL.Exceptions
{
    [Serializable]
    public class ReaderException : LexerException
    {
        public ReaderException(IContextProvider context, string message)
            : base(context, message)
        {
        }
    }
}
