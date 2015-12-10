using System;
using System.Runtime.Serialization;

namespace GDDL
{
    [Serializable]
    internal class ReaderException : Exception
    {
        public ReaderException(Reader context, string message)
            : base(string.Format("{0}: {1}", context.GetFileContext(), message))
        {
        }
    }
}