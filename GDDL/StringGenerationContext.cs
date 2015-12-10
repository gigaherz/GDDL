using System;

namespace GDDL
{
    public class StringGenerationContext
    {
        public StringGenerationOptions Options;

        public int IndentLevel = 0;

        public StringGenerationContext(StringGenerationOptions options)
        {
            Options = options;
        }
    }

    [Flags]
    public enum StringGenerationOptions
    {
        None = 0,

        // Decoration level
        Compact = 0,
        Nice = 1,

    }
}
