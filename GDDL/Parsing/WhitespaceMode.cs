using System;
using System.Collections.Generic;
using System.Text;

namespace GDDL.Parsing
{
    public enum WhitespaceMode
    {
        /// Ignores all whitespace and comments
        DropAll,

        /// Preserves comments, but not
        PreserveComments,

        /// Preserves comments and whitespace
        PreserveAllWhitespace
    }
}
