namespace GDDL.Config
{
    public class StringGenerationOptions
    {
        public static readonly StringGenerationOptions Compact = new StringGenerationOptions(); // Default
        public static readonly StringGenerationOptions Nice = new StringGenerationOptions();

        static StringGenerationOptions() {
            Nice.writeComments = true;
            Nice.lineBreaksAfterOpeningBrace = 1;
            Nice.lineBreaksBeforeClosingBrace = 1;
            Nice.lineBreaksAfterClosingBrace = 1;
            Nice.lineBreaksAfterValues = 1;
            Nice.spacesBeforeOpeningBrace = 0;
            Nice.spacesAfterOpeningBrace = 1;
            Nice.spacesBeforeClosingBrace = 1;
            Nice.spacesAfterClosingBrace = 0;
            Nice.spacesBetweenElements = 1;
            Nice.oneElementPerLineThreshold = 10;
            Nice.spacesPerIndent = 4;
        }

        // Sets
        public int lineBreaksBeforeOpeningBrace = 0;
        public int lineBreaksAfterOpeningBrace = 0;
        public int lineBreaksBeforeClosingBrace = 0;
        public int lineBreaksAfterClosingBrace = 0;
        public int spacesBeforeOpeningBrace = 0;
        public int spacesAfterOpeningBrace = 0;
        public int spacesBeforeClosingBrace = 0;
        public int spacesAfterClosingBrace = 0;
        public int oneElementPerLineThreshold = int.MaxValue;
        public int spacesBetweenElements = 1;

        // Values
        public int lineBreaksAfterValues = 0;

        // Naming
        public bool alwaysQuoteNames = false;
        public int lineBreaksAfterName = 0;

        // Typing
        public bool alwaysQuoteTypes = false;
        public int lineBreaksAfterType = 0;

        // Indentation
        public bool indentUsingTabs = false;
        public int spacesPerIndent = 2;
        public int indentSetContents = 0;
        public int indentExtraLines = 0;

        // Comments
        public bool writeComments = false;
    }
}