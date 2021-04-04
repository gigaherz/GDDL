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

        // Internal Constructor
        private FormatterOptions(Builder builder)
        {
            lineBreaksBeforeOpeningBrace = builder._lineBreaksBeforeOpeningBrace;
            lineBreaksAfterOpeningBrace = builder._lineBreaksAfterOpeningBrace;
            lineBreaksBeforeClosingBrace = builder._lineBreaksBeforeClosingBrace;
            lineBreaksAfterClosingBrace = builder._lineBreaksAfterClosingBrace;
            spacesBeforeOpeningBrace = builder._spacesBeforeOpeningBrace;
            spacesAfterOpeningBrace = builder._spacesAfterOpeningBrace;
            spacesBeforeClosingBrace = builder._spacesBeforeClosingBrace;
            spacesAfterClosingBrace = builder._spacesAfterClosingBrace;
            oneElementPerLineThreshold = builder._oneElementPerLineThreshold;
            spacesBetweenElements = builder._spacesBetweenElements;
            omitCommaAfterClosingBrace = builder._omitCommaAfterClosingBrace;
            lineBreaksAfterValues = builder._lineBreaksAfterValues;
            floatFormattingStyle = builder._floatFormattingStyle;
            alwaysShowNumberSign = builder._alwaysShowNumberSign;
            alwaysShowExponentSign = builder._alwaysShowExponentSign;
            autoScientificNotationUpper = builder._autoScientificNotationUpper;
            autoScientificNotationLower = builder._autoScientificNotationLower;
            floatSignificantFigures = builder._floatSignificantFigures;
            indentUsingTabs = builder._indentUsingTabs;
            spacesPerIndent = builder._spacesPerIndent;
            writeComments = builder._writeComments;
            blankLinesBeforeComment = builder._blankLinesBeforeComment;
        }

        public class Builder
        {
            // Collections
            internal protected int _lineBreaksBeforeOpeningBrace = 0;
            internal protected int _lineBreaksAfterOpeningBrace = 0;
            internal protected int _lineBreaksBeforeClosingBrace = 0;
            internal protected int _lineBreaksAfterClosingBrace = 0;
            internal protected int _spacesBeforeOpeningBrace = 0;
            internal protected int _spacesAfterOpeningBrace = 0;
            internal protected int _spacesBeforeClosingBrace = 0;
            internal protected int _spacesAfterClosingBrace = 0;
            internal protected int _oneElementPerLineThreshold = int.MaxValue;
            internal protected int _spacesBetweenElements = 1;
            internal protected bool _omitCommaAfterClosingBrace = false;

            // Values
            internal protected int _lineBreaksAfterValues = 0;
            internal protected DoubleFormattingStyle _floatFormattingStyle = DoubleFormattingStyle.Auto;
            internal protected bool _alwaysShowNumberSign = false;
            internal protected bool _alwaysShowExponentSign = false;
            internal protected int _autoScientificNotationUpper = 5;
            internal protected int _autoScientificNotationLower = -2;
            internal protected int _floatSignificantFigures = 15;

            // Indentation
            internal protected bool _indentUsingTabs = false;
            internal protected int _spacesPerIndent = 2;

            // Comments
            internal protected bool _writeComments = false;
            internal protected int _blankLinesBeforeComment = 0;

            public Builder LineBreaksBeforeOpeningBrace(int lineBreaksBeforeOpeningBrace)
            {
                this._lineBreaksBeforeOpeningBrace = lineBreaksBeforeOpeningBrace;
                return this;
            }

            public Builder LineBreaksAfterOpeningBrace(int lineBreaksAfterOpeningBrace)
            {
                this._lineBreaksAfterOpeningBrace = lineBreaksAfterOpeningBrace;
                return this;
            }

            public Builder LineBreaksBeforeClosingBrace(int lineBreaksBeforeClosingBrace)
            {
                this._lineBreaksBeforeClosingBrace = lineBreaksBeforeClosingBrace;
                return this;
            }

            public Builder LineBreaksAfterClosingBrace(int lineBreaksAfterClosingBrace)
            {
                this._lineBreaksAfterClosingBrace = lineBreaksAfterClosingBrace;
                return this;
            }

            public Builder SpacesBeforeOpeningBrace(int spacesBeforeOpeningBrace)
            {
                this._spacesBeforeOpeningBrace = spacesBeforeOpeningBrace;
                return this;
            }

            public Builder SpacesAfterOpeningBrace(int spacesAfterOpeningBrace)
            {
                this._spacesAfterOpeningBrace = spacesAfterOpeningBrace;
                return this;
            }

            public Builder SpacesBeforeClosingBrace(int spacesBeforeClosingBrace)
            {
                this._spacesBeforeClosingBrace = spacesBeforeClosingBrace;
                return this;
            }

            public Builder SpacesAfterClosingBrace(int spacesAfterClosingBrace)
            {
                this._spacesAfterClosingBrace = spacesAfterClosingBrace;
                return this;
            }

            public Builder OneElementPerLineThreshold(int oneElementPerLineThreshold)
            {
                this._oneElementPerLineThreshold = oneElementPerLineThreshold;
                return this;
            }

            public Builder SpacesBetweenElements(int spacesBetweenElements)
            {
                this._spacesBetweenElements = spacesBetweenElements;
                return this;
            }

            public Builder OmitCommaAfterClosingBrace(bool omitCommaAfterClosingBrace)
            {
                this._omitCommaAfterClosingBrace = omitCommaAfterClosingBrace;
                return this;
            }

            public Builder LineBreaksAfterValues(int lineBreaksAfterValues)
            {
                this._lineBreaksAfterValues = lineBreaksAfterValues;
                return this;
            }

            public Builder FloatFormattingStyle(DoubleFormattingStyle floatFormattingStyle)
            {
                this._floatFormattingStyle = floatFormattingStyle;
                return this;
            }

            public Builder AlwaysShowNumberSign(bool alwaysShowNumberSign)
            {
                this._alwaysShowNumberSign = alwaysShowNumberSign;
                return this;
            }

            public Builder AlwaysShowExponentSign(bool alwaysShowExponentSign)
            {
                this._alwaysShowExponentSign = alwaysShowExponentSign;
                return this;
            }

            public Builder AutoScientificNotationUpper(int autoScientificNotationUpper)
            {
                this._autoScientificNotationUpper = autoScientificNotationUpper;
                return this;
            }

            public Builder AutoScientificNotationLower(int autoScientificNotationLower)
            {
                this._autoScientificNotationLower = autoScientificNotationLower;
                return this;
            }

            public Builder FloatSignificantFigures(int floatSignificantFigures)
            {
                this._floatSignificantFigures = floatSignificantFigures;
                return this;
            }

            public Builder IndentUsingTabs(bool indentUsingTabs)
            {
                this._indentUsingTabs = indentUsingTabs;
                return this;
            }

            public Builder SpacesPerIndent(int spacesPerIndent)
            {
                this._spacesPerIndent = spacesPerIndent;
                return this;
            }

            public Builder WriteComments(bool writeComments)
            {
                this._writeComments = writeComments;
                return this;
            }

            internal Builder BlankLinesBeforeComment(int blankLinesBeforeComment)
            {
                this._blankLinesBeforeComment = blankLinesBeforeComment;
                return this;
            }

            public FormatterOptions Build()
            {
                return new FormatterOptions(this);
            }
        }
    }

}
