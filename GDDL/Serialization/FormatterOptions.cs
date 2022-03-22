
namespace GDDL.Serialization
{
    public class FormatterOptions
    {
        public static readonly FormatterOptions Compact = new Builder().Build(); // Default
        public static readonly FormatterOptions Nice = new Builder()
                .WriteComments(true)
                .LineBreaksAfterOpeningBrace(1)
                .LineBreaksBeforeClosingBrace(1)
                .LineBreaksAfterClosingBrace(1)
                .LineBreaksAfterValues(1)
                .SpacesBeforeOpeningBrace(0)
                .SpacesAfterOpeningBrace(1)
                .SpacesBeforeClosingBrace(1)
                .SpacesAfterClosingBrace(0)
                .SpacesInEmptyCollection(1)
                .SpacesAfterComma(1)
                .SpacesBeforeEquals(1)
                .SpacesAfterEquals(1)
                .SpacesInEmptyCollection(1)
                .OneElementPerLineThreshold(10)
                .SpacesPerIndent(4)
                .BlankLinesBeforeComment(1)
                .Build();
        public static readonly FormatterOptions CompactJson = new Builder(Compact).AlwaysUseStringLiterals(true).UseJsonDelimiters(true).Build();
        public static readonly FormatterOptions NiceJson = new Builder(Nice).SpacesBeforeEquals(0).AlwaysUseStringLiterals(true).UseJsonDelimiters(true).Build();
        public static readonly FormatterOptions CompactJson5 = new Builder(CompactJson).AlwaysUseStringLiterals(false).Build();
        public static readonly FormatterOptions NiceJson5 = new Builder(NiceJson).AlwaysUseStringLiterals(false).Build();

        // Collections
        public readonly int lineBreaksBeforeOpeningBrace;
        public readonly int lineBreaksAfterOpeningBrace;
        public readonly int lineBreaksBeforeClosingBrace;
        public readonly int lineBreaksAfterClosingBrace;
        public readonly int spacesBeforeOpeningBrace;
        public readonly int spacesAfterOpeningBrace;
        public readonly int spacesBeforeClosingBrace;
        public readonly int spacesAfterClosingBrace;
        public readonly int spacesBeforeComma;
        public readonly int spacesAfterComma;
        public readonly int spacesBeforeEquals;
        public readonly int spacesAfterEquals;
        public readonly int spacesInEmptyCollection;
        public readonly int oneElementPerLineThreshold;
        public readonly bool omitCommaAfterClosingBrace;
        public readonly bool sortMapKeys;
        public readonly bool alwaysUseStringLiterals;

        // Values
        public readonly int lineBreaksAfterValues;
        public readonly DoubleFormattingStyle floatFormattingStyle;
        public readonly bool alwaysShowNumberSign;
        public readonly bool alwaysShowExponentSign;
        public readonly int autoScientificNotationUpper;
        public readonly int autoScientificNotationLower;
        public readonly int floatSignificantFigures;

        // Indentation
        public readonly bool indentUsingTabs;
        public readonly int spacesPerIndent;

        // Comments
        public readonly bool writeComments;
        public readonly int blankLinesBeforeComment;
        public readonly bool trimCommentLines;

        // Other
        public readonly bool useJsonDelimiters;

        // Internal Constructor
        private FormatterOptions(Builder builder)
        {
            lineBreaksBeforeOpeningBrace = builder.lineBreaksBeforeOpeningBrace;
            lineBreaksAfterOpeningBrace = builder.lineBreaksAfterOpeningBrace;
            lineBreaksBeforeClosingBrace = builder.lineBreaksBeforeClosingBrace;
            lineBreaksAfterClosingBrace = builder.lineBreaksAfterClosingBrace;
            spacesBeforeOpeningBrace = builder.spacesBeforeOpeningBrace;
            spacesAfterOpeningBrace = builder.spacesAfterOpeningBrace;
            spacesBeforeClosingBrace = builder.spacesBeforeClosingBrace;
            spacesAfterClosingBrace = builder.spacesAfterClosingBrace;
            spacesBeforeComma = builder.spacesBeforeComma;
            spacesAfterComma = builder.spacesAfterComma;
            spacesBeforeEquals = builder.spacesBeforeEquals;
            spacesAfterEquals = builder.spacesAfterEquals;
            spacesInEmptyCollection = builder.spacesInEmptyCollection;
            oneElementPerLineThreshold = builder.oneElementPerLineThreshold;
            omitCommaAfterClosingBrace = builder.omitCommaAfterClosingBrace;
            sortMapKeys = builder.sortMapKeys;
            lineBreaksAfterValues = builder.lineBreaksAfterValues;
            floatFormattingStyle = builder.floatFormattingStyle;
            alwaysShowNumberSign = builder.alwaysShowNumberSign;
            alwaysShowExponentSign = builder.alwaysShowExponentSign;
            autoScientificNotationUpper = builder.autoScientificNotationUpper;
            autoScientificNotationLower = builder.autoScientificNotationLower;
            floatSignificantFigures = builder.floatSignificantFigures;
            indentUsingTabs = builder.indentUsingTabs;
            spacesPerIndent = builder.spacesPerIndent;
            writeComments = builder.writeComments;
            blankLinesBeforeComment = builder.blankLinesBeforeComment;
            trimCommentLines = builder.trimCommentLines;
            useJsonDelimiters = builder.useJsonDelimiters;
            alwaysUseStringLiterals = builder.alwaysUseStringLiterals;
        }

        public class Builder
        {
            // Collections
            protected internal int lineBreaksBeforeOpeningBrace = 0;
            protected internal int lineBreaksAfterOpeningBrace = 0;
            protected internal int lineBreaksBeforeClosingBrace = 0;
            protected internal int lineBreaksAfterClosingBrace = 0;
            protected internal int spacesBeforeOpeningBrace = 0;
            protected internal int spacesAfterOpeningBrace = 0;
            protected internal int spacesBeforeClosingBrace = 0;
            protected internal int spacesAfterClosingBrace = 0;
            protected internal int spacesBeforeComma = 0;
            protected internal int spacesAfterComma = 0;
            protected internal int spacesBeforeEquals = 0;
            protected internal int spacesAfterEquals = 0;
            protected internal int spacesInEmptyCollection = 0;
            protected internal int oneElementPerLineThreshold = int.MaxValue;
            protected internal bool omitCommaAfterClosingBrace = false;
            protected internal bool sortMapKeys = false;
            protected internal bool alwaysUseStringLiterals = false;

            // Values
            protected internal int lineBreaksAfterValues = 0;
            protected internal DoubleFormattingStyle floatFormattingStyle = DoubleFormattingStyle.Auto;
            protected internal bool alwaysShowNumberSign = false;
            protected internal bool alwaysShowExponentSign = false;
            protected internal int autoScientificNotationUpper = 5;
            protected internal int autoScientificNotationLower = -2;
            protected internal int floatSignificantFigures = 15;

            // Indentation
            protected internal bool indentUsingTabs = false;
            protected internal int spacesPerIndent = 2;

            // Comments
            protected internal bool writeComments = false;
            protected internal int blankLinesBeforeComment = 0;
            protected internal bool trimCommentLines = true;

            // Other
            protected internal bool useJsonDelimiters = false;

            public Builder()
            {
            }

            public Builder(FormatterOptions copyFrom)
            {
                lineBreaksBeforeOpeningBrace = copyFrom.lineBreaksBeforeOpeningBrace;
                lineBreaksAfterOpeningBrace = copyFrom.lineBreaksAfterOpeningBrace;
                lineBreaksBeforeClosingBrace = copyFrom.lineBreaksBeforeClosingBrace;
                lineBreaksAfterClosingBrace = copyFrom.lineBreaksAfterClosingBrace;
                spacesBeforeOpeningBrace = copyFrom.spacesBeforeOpeningBrace;
                spacesAfterOpeningBrace = copyFrom.spacesAfterOpeningBrace;
                spacesBeforeClosingBrace = copyFrom.spacesBeforeClosingBrace;
                spacesAfterClosingBrace = copyFrom.spacesAfterClosingBrace;
                spacesBeforeComma = copyFrom.spacesBeforeComma;
                spacesAfterComma = copyFrom.spacesAfterComma;
                spacesBeforeEquals = copyFrom.spacesBeforeEquals;
                spacesAfterEquals = copyFrom.spacesAfterEquals;
                spacesInEmptyCollection = copyFrom.spacesInEmptyCollection;
                oneElementPerLineThreshold = copyFrom.oneElementPerLineThreshold;
                omitCommaAfterClosingBrace = copyFrom.omitCommaAfterClosingBrace;
                sortMapKeys = copyFrom.sortMapKeys;
                lineBreaksAfterValues = copyFrom.lineBreaksAfterValues;
                floatFormattingStyle = copyFrom.floatFormattingStyle;
                alwaysShowNumberSign = copyFrom.alwaysShowNumberSign;
                alwaysShowExponentSign = copyFrom.alwaysShowExponentSign;
                autoScientificNotationUpper = copyFrom.autoScientificNotationUpper;
                autoScientificNotationLower = copyFrom.autoScientificNotationLower;
                floatSignificantFigures = copyFrom.floatSignificantFigures;
                indentUsingTabs = copyFrom.indentUsingTabs;
                spacesPerIndent = copyFrom.spacesPerIndent;
                writeComments = copyFrom.writeComments;
                blankLinesBeforeComment = copyFrom.blankLinesBeforeComment;
                trimCommentLines = copyFrom.trimCommentLines;
                useJsonDelimiters = copyFrom.useJsonDelimiters;
            }

            public Builder LineBreaksBeforeOpeningBrace(int lineBreaksBeforeOpeningBrace)
            {
                this.lineBreaksBeforeOpeningBrace = lineBreaksBeforeOpeningBrace;
                return this;
            }

            public Builder LineBreaksAfterOpeningBrace(int lineBreaksAfterOpeningBrace)
            {
                this.lineBreaksAfterOpeningBrace = lineBreaksAfterOpeningBrace;
                return this;
            }

            public Builder LineBreaksBeforeClosingBrace(int lineBreaksBeforeClosingBrace)
            {
                this.lineBreaksBeforeClosingBrace = lineBreaksBeforeClosingBrace;
                return this;
            }

            public Builder LineBreaksAfterClosingBrace(int lineBreaksAfterClosingBrace)
            {
                this.lineBreaksAfterClosingBrace = lineBreaksAfterClosingBrace;
                return this;
            }

            public Builder SpacesBeforeOpeningBrace(int spacesBeforeOpeningBrace)
            {
                this.spacesBeforeOpeningBrace = spacesBeforeOpeningBrace;
                return this;
            }

            public Builder SpacesAfterOpeningBrace(int spacesAfterOpeningBrace)
            {
                this.spacesAfterOpeningBrace = spacesAfterOpeningBrace;
                return this;
            }

            public Builder SpacesBeforeClosingBrace(int spacesBeforeClosingBrace)
            {
                this.spacesBeforeClosingBrace = spacesBeforeClosingBrace;
                return this;
            }

            public Builder SpacesAfterClosingBrace(int spacesAfterClosingBrace)
            {
                this.spacesAfterClosingBrace = spacesAfterClosingBrace;
                return this;
            }

            public Builder SpacesBeforeComma(int spacesBeforeComma)
            {
                this.spacesBeforeComma = spacesBeforeComma;
                return this;
            }

            public Builder SpacesAfterComma(int spacesAfterComma)
            {
                this.spacesAfterComma = spacesAfterComma;
                return this;
            }

            public Builder SpacesBeforeEquals(int spacesBeforeEquals)
            {
                this.spacesBeforeEquals = spacesBeforeEquals;
                return this;
            }

            public Builder SpacesAfterEquals(int spacesAfterEquals)
            {
                this.spacesAfterEquals = spacesAfterEquals;
                return this;
            }

            public Builder SpacesInEmptyCollection(int spacesInEmptyCollection)
            {
                this.spacesInEmptyCollection = spacesInEmptyCollection;
                return this;
            }

            public Builder OneElementPerLineThreshold(int oneElementPerLineThreshold)
            {
                this.oneElementPerLineThreshold = oneElementPerLineThreshold;
                return this;
            }

            public Builder OmitCommaAfterClosingBrace(bool omitCommaAfterClosingBrace)
            {
                this.omitCommaAfterClosingBrace = omitCommaAfterClosingBrace;
                return this;
            }

            public Builder SortMapKeys(bool sortMapKeys)
            {
                this.sortMapKeys = sortMapKeys;
                return this;
            }

            public Builder LineBreaksAfterValues(int lineBreaksAfterValues)
            {
                this.lineBreaksAfterValues = lineBreaksAfterValues;
                return this;
            }

            public Builder FloatFormattingStyle(DoubleFormattingStyle floatFormattingStyle)
            {
                this.floatFormattingStyle = floatFormattingStyle;
                return this;
            }

            public Builder AlwaysShowNumberSign(bool alwaysShowNumberSign)
            {
                this.alwaysShowNumberSign = alwaysShowNumberSign;
                return this;
            }

            public Builder AlwaysShowExponentSign(bool alwaysShowExponentSign)
            {
                this.alwaysShowExponentSign = alwaysShowExponentSign;
                return this;
            }

            public Builder AutoScientificNotationUpper(int autoScientificNotationUpper)
            {
                this.autoScientificNotationUpper = autoScientificNotationUpper;
                return this;
            }

            public Builder AutoScientificNotationLower(int autoScientificNotationLower)
            {
                this.autoScientificNotationLower = autoScientificNotationLower;
                return this;
            }

            public Builder FloatSignificantFigures(int floatSignificantFigures)
            {
                this.floatSignificantFigures = floatSignificantFigures;
                return this;
            }

            public Builder IndentUsingTabs(bool indentUsingTabs)
            {
                this.indentUsingTabs = indentUsingTabs;
                return this;
            }

            public Builder SpacesPerIndent(int spacesPerIndent)
            {
                this.spacesPerIndent = spacesPerIndent;
                return this;
            }

            public Builder WriteComments(bool writeComments)
            {
                this.writeComments = writeComments;
                return this;
            }

            internal Builder BlankLinesBeforeComment(int blankLinesBeforeComment)
            {
                this.blankLinesBeforeComment = blankLinesBeforeComment;
                return this;
            }

            public Builder TrimCommentLines(bool trimCommentLines)
            {
                this.trimCommentLines = trimCommentLines;
                return this;
            }

            public Builder UseJsonDelimiters(bool useJsonDelimiters)
            {
                this.useJsonDelimiters = useJsonDelimiters;
                return this;
            }

            public Builder AlwaysUseStringLiterals(bool alwaysUseStringLiterals)
            {
                this.alwaysUseStringLiterals = alwaysUseStringLiterals;
                return this;
            }

            public FormatterOptions Build()
            {
                return new FormatterOptions(this);
            }
        }
    }

}
