using System;
using System.Runtime.Serialization;

namespace GDDL
{
    [Serializable]
    internal class ReaderErrorException : Exception
    {
        public ReaderErrorException(Reader context)
            : base(context.GetFileContext().ToString())
        {
        }

        public ReaderErrorException(Reader context, string message)
            : base(string.Format("{0}: {1}", context.GetFileContext(), message))
        {
        }
    }
}