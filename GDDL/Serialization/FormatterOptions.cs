namespace GDDL.Serialization
{
    public class FormatterOptions
    {
        public static readonly FormatterOptions Compact = new FormatterOptions(); // Default
        public static readonly FormatterOptions Nice = new FormatterOptions();

        static FormatterOptions()
        {
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

        // Collections
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
        public bool omitCommaAfterClosingBrace = false;

        // Values
        public int lineBreaksAfterValues = 0;
        public DoubleFormattingStyle floatFormattingStyle = DoubleFormattingStyle.Auto;
        public bool alwaysShowNumberSign = false;
        public bool alwaysShowExponentSign = false;
        public int autoScientificNotationUpper = 5;
        public int autoScientificNotationLower = -2;
        public int floatSignificantFigures = 15;

        // Indentation
        public bool indentUsingTabs = false;
        public int spacesPerIndent = 2;

        // Comments
        public bool writeComments = false;

    }
}
