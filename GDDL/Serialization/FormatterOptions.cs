using System;

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
                    .SpacesBetweenElements(1)
                    .OneElementPerLineThreshold(10)
                    .SpacesPerIndent(4)
                    .BlankLinesBeforeComment(1)
                .Build();

        // Collections
        public readonly int lineBreaksBeforeOpeningBrace;
        public readonly int lineBreaksAfterOpeningBrace;
        public readonly int lineBreaksBeforeClosingBrace;
        public readonly int lineBreaksAfterClosingBrace;
        public readonly int spacesBeforeOpeningBrace;
        public readonly int spacesAfterOpeningBrace;
        public readonly int spacesBeforeClosingBrace;
        public readonly int spacesAfterClosingBrace;
        public readonly int oneElementPerLineThreshold;
        public readonly int spacesBetweenElements;
        public readonly bool omitCommaAfterClosingBrace;

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
            oneElementPerLineThreshold = builder.oneElementPerLineThreshold;
            spacesBetweenElements = builder.spacesBetweenElements;
            omitCommaAfterClosingBrace = builder.omitCommaAfterClosingBrace;
            lineBreaksAfterValues = builder.lineBreaksAfterValues;
            floatFormattingStyle = builder.floatFormattingStyle;
            alwaysShowNumberSign = builder.alwaysShowNumberSign;
            alwaysShowExponentSign = builder._alwaysShowExponentSign;
            autoScientificNotationUpper = builder.autoScientificNotationUpper;
            autoScientificNotationLower = builder.autoScientificNotationLower;
            floatSignificantFigures = builder.floatSignificantFigures;
            indentUsingTabs = builder.indentUsingTabs;
            spacesPerIndent = builder.spacesPerIndent;
            writeComments = builder.writeComments;
            blankLinesBeforeComment = builder.blankLinesBeforeComment;
            trimCommentLines = builder.trimCommentLines;
        }

        public class Builder
        {
            // Collections
            internal protected int lineBreaksBeforeOpeningBrace = 0;
            internal protected int lineBreaksAfterOpeningBrace = 0;
            internal protected int lineBreaksBeforeClosingBrace = 0;
            internal protected int lineBreaksAfterClosingBrace = 0;
            internal protected int spacesBeforeOpeningBrace = 0;
            internal protected int spacesAfterOpeningBrace = 0;
            internal protected int spacesBeforeClosingBrace = 0;
            internal protected int spacesAfterClosingBrace = 0;
            internal protected int oneElementPerLineThreshold = int.MaxValue;
            internal protected int spacesBetweenElements = 1;
            internal protected bool omitCommaAfterClosingBrace = false;

            // Values
            internal protected int lineBreaksAfterValues = 0;
            internal protected DoubleFormattingStyle floatFormattingStyle = DoubleFormattingStyle.Auto;
            internal protected bool alwaysShowNumberSign = false;
            internal protected bool _alwaysShowExponentSign = false;
            internal protected int autoScientificNotationUpper = 5;
            internal protected int autoScientificNotationLower = -2;
            internal protected int floatSignificantFigures = 15;

            // Indentation
            internal protected bool indentUsingTabs = false;
            internal protected int spacesPerIndent = 2;

            // Comments
            internal protected bool writeComments = false;
            internal protected int blankLinesBeforeComment = 0;
            internal protected bool trimCommentLines = true;

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

            public Builder OneElementPerLineThreshold(int oneElementPerLineThreshold)
            {
                this.oneElementPerLineThreshold = oneElementPerLineThreshold;
                return this;
            }

            public Builder SpacesBetweenElements(int spacesBetweenElements)
            {
                this.spacesBetweenElements = spacesBetweenElements;
                return this;
            }

            public Builder OmitCommaAfterClosingBrace(bool omitCommaAfterClosingBrace)
            {
                this.omitCommaAfterClosingBrace = omitCommaAfterClosingBrace;
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
                this._alwaysShowExponentSign = alwaysShowExponentSign;
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

            public FormatterOptions Build()
            {
                return new FormatterOptions(this);
            }
        }
    }

}
