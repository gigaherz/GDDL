using GDDL.Structure;
using GDDL.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GDDL.Serialization
{
    public class Formatter
    {
        #region API

        public static string FormatCompact(GddlDocument doc)
        {
            return Format(doc, FormatterOptions.Compact);
        }

        public static string FormatCompact(GddlElement element)
        {
            return Format(element, FormatterOptions.Compact);
        }

        public static string FormatNice(GddlDocument doc)
        {
            return Format(doc, FormatterOptions.Nice);
        }

        public static string FormatNice(GddlElement element)
        {
            return Format(element, FormatterOptions.Nice);
        }

        public static string Format(GddlDocument doc, FormatterOptions options)
        {
            var b = new StringBuilder();
            var f = new Formatter(b, options);
            f.FormatDocument(doc);
            return b.ToString();
        }

        public static string Format(GddlElement element, FormatterOptions options)
        {
            var b = new StringBuilder();
            var f = new Formatter(b, options);
            f.FormatElement(element);
            return b.ToString();
        }

        public Formatter(StringBuilder builder, FormatterOptions options)
        {
            this.builder = builder;
            this.Options = options;
        }

        public FormatterOptions Options { get; }

        public void FormatDocument(GddlDocument doc)
        {
            FormatElement(doc.Root);

            if (doc.HasDanglingComment && Options.writeComments)
                FormatComment(doc.DanglingComment);
        }

        public void FormatElement(GddlElement element)
        {
            FormatComment(element);
            AppendIndent();
            FormatElement(element, false);
        }

        #endregion

        #region Implementation

        private static readonly Regex CommentLineSplitter = new("(?:(?:\n)|(?:\r\n))");

        private readonly Stack<int> indentLevels = new();
        private readonly StringBuilder builder;

        private int indentLevel = 0;

        private void PushIndent()
        {
            indentLevels.Push(indentLevel);
        }

        private void PopIndent()
        {
            indentLevel = indentLevels.Pop();
        }

        private void ClearIndent()
        {
            indentLevel = 0;
        }

        private void IncIndent()
        {
            indentLevel++;
        }

        private void AppendMultiple(char c, int n)
        {
            for (int i = 0; i < n; i++)
                builder.Append(c);
        }

        private void AppendIndent()
        {
            int tabsToGen = indentLevel;
            for (int i = 0; i < tabsToGen; i++)
            {
                if (Options.indentUsingTabs)
                {
                    builder.Append('\t');
                }
                else
                {
                    AppendMultiple(' ', Options.spacesPerIndent);
                }
            }
        }

        protected void FormatComment(GddlElement e)
        {
            if (e.HasComment && Options.writeComments)
            {
                FormatComment(e.Comment);
            }
        }

        private void FormatComment(string comment)
        {
            AppendMultiple('\n', Options.blankLinesBeforeComment);
            string[] lines = CommentLineSplitter.Split(comment);
            int count = lines.Length;
            if (count > 0 && Options.trimCommentLines)
            {
                while (lines[count - 1].Length == 0)
                    count--;
            }

            for (int i = 0; i < count; i++)
            {
                AppendIndent();
                builder.Append('#');
                builder.Append(lines[i]);
                builder.Append('\n');
            }
        }

        protected void FormatElement(GddlElement element, bool hasNext)
        {
            switch (element)
            {
            case GddlValue v:
                FormatValue(v);
                break;
            case GddlReference r:
                FormatReference(r);
                break;
            case GddlMap m:
                FormatMap(m, hasNext);
                break;
            case GddlList l:
                FormatList(l, hasNext);
                break;
            default:
                throw new NotImplementedException(
                    "A new Element type has been added without updating Formatter#FormatElement.");
            }
        }

        protected void FormatValue(GddlValue v)
        {
            if (v.IsNull)
            {
                builder.Append("null");
            }
            else if (v.IsBoolean)
            {
                builder.Append(v.AsBoolean ? "true" : "false");
            }
            else if (v.IsInteger)
            {
                FormatInteger(v.AsInteger);
            }
            else if (v.IsDouble)
            {
                FormatDoubleCustom(v.AsDouble);
            }
            else if (v.IsString)
            {
                builder.Append(Utility.EscapeString(v.AsString));
            }
            else
            {
                throw new NotImplementedException(
                    "A new Value type has been added without updating Formatter#formatValue.");
            }
        }

        protected void FormatInteger(long value)
        {
            builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}", value));
        }

        protected void FormatDoubleCustom(double value)
        {
            switch (Options.floatFormattingStyle)
            {
            case DoubleFormattingStyle.Decimal:
                FormatDoubleDecimal(value);
                break;
            case DoubleFormattingStyle.Scientific:
                FormatDoubleScientific(value);
                break;
            default:
                FormatDoubleAuto(value);
                break;
            }
        }

        protected void FormatDoubleAuto(double value)
        {
            if (FormatSpecial(value))
                return;

            int exp = (int)Math.Floor(Math.Log10(Math.Abs(value)));
            if (exp >= Options.autoScientificNotationUpper || exp < Options.autoScientificNotationLower)
            {
                FormatDoubleScientific(value);
            }
            else
            {
                FormatDoubleDecimal(value);
            }
        }

        protected void FormatDoubleScientific(double value)
        {
            if (FormatSpecial(value))
                return;

            int exp = (int)Math.Floor(Math.Log10(Math.Abs(value)));
            double adjusted = value / Math.Pow(10, exp);
            FormatDoubleDecimal(adjusted);
            builder.Append('e');
            if (Options.alwaysShowExponentSign)
                FormatSign(exp);
            else
                FormatNegative(exp);
            FormatInteger(Math.Abs(exp));
        }

        protected void FormatDoubleDecimal(double value)
        {
            if (Options.alwaysShowNumberSign)
                FormatSign(value);
            else
                FormatNegative(value);
            value = Math.Abs(value);

            double integral = Math.Floor(value);
            double fractional = value - integral;

            var temp = new List<int>();

            int intDigits = FormatIntegral(integral, temp);

            builder.Append('.');

            FormatFractional(fractional, intDigits, temp);
        }

        internal int FormatIntegral(double integral, List<int> temp)
        {
            if (!(integral > 0))
            {
                builder.Append('0');
                return 0;
            }

            int exp = (int)Math.Ceiling(Math.Log10(integral));
            double value = integral / Math.Pow(10, exp);

            int nonTrailingDigits = FormatDigits(temp, Math.Min(exp, Options.floatSignificantFigures), value);

            AppendMultiple('0', exp - nonTrailingDigits);
            return exp;
        }

        internal void FormatFractional(double fractional, int intDigits, List<int> temp)
        {
            FormatDigits(temp, (Options.floatSignificantFigures - intDigits), fractional);
        }

        private int FormatDigits(List<int> temp, int exp, double value)
        {
            temp.Clear();
            while (value > 0 && temp.Count < exp)
            {
                value *= 10;
                int digit = (int)Math.Floor(value);
                value -= digit;
                temp.Add(digit);
            }

            if (temp.Count == 0)
            {
                temp.Add(0);
            }

            int nonTrailingDigits = RoundDigits(temp, value);
            for (int i = 0; i < nonTrailingDigits; i++)
            {
                builder.Append((char)('0' + temp[i]));
            }

            return nonTrailingDigits;
        }

        private static int RoundDigits(List<int> temp, double value)
        {
            int l = temp.Count - 1;
            int r = value >= 0.5 ? 1 : 0;
            while (r > 0 && l >= 0) // round up
            {
                int v = temp[l];
                v++;
                if (v >= 10)
                {
                    //r = 1;
                    v -= 10;
                }
                else
                {
                    r = 0;
                }

                temp[l] = v;
                l--;
            }

            int firstTrailingZero = temp.Count;
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                if (temp[i] != 0)
                {
                    firstTrailingZero = i + 1;
                    break;
                }
            }

            return firstTrailingZero;
        }

        private bool FormatSpecial(double value)
        {
            if (double.IsNaN(value))
            {
                builder.Append(".NaN");
                return true;
            }

            if (!double.IsFinite(value))
            {
                if (Options.alwaysShowNumberSign)
                    FormatSign(value);
                else
                    FormatNegative(value);
                builder.Append(".Inf");
                return true;
            }

            return false;
        }

        private void FormatNegative(double value)
        {
            long l = BitConverter.DoubleToInt64Bits(value);
            if (l < 0) builder.Append('-');
        }

        private void FormatSign(double value)
        {
            long l = BitConverter.DoubleToInt64Bits(value);
            builder.Append(l < 0 ? "-" : "+");
        }

        internal void FormatReference(GddlReference r)
        {
            int count = 0;
            foreach (var it in r.NameParts)
            {
                if (count++ > 0)
                    builder.Append(Options.useJsonDelimiters ? '/' : ':');
                builder.Append(it.ToString(this));
            }

            /*if (r.IsResolved)
            {
                builder.Append('=');
                if (r.ResolvedValue == null)
                    builder.Append("NULL");
                else
                    builder.Append(r.ResolvedValue);
            }*/
        }

        internal void FormatMap(GddlMap c, bool hasNext0)
        {
            PushIndent();

            bool oneElementPerLine = c.GetFormattingComplexity() > Options.oneElementPerLineThreshold;

            if (c.HasTypeName)
            {
                builder.Append(c.TypeName);
                if (Options.lineBreaksBeforeOpeningBrace == 0)
                    builder.Append(' ');
            }

            if (oneElementPerLine && Options.lineBreaksBeforeOpeningBrace > 0)
            {
                AppendMultiple('\n', Options.lineBreaksBeforeOpeningBrace);
                AppendIndent();
            }
            else if (Options.spacesBeforeOpeningBrace > 0)
            {
                AppendMultiple(' ', Options.spacesBeforeOpeningBrace);
            }

            builder.Append('{');
            if (c.Count == 0 && !oneElementPerLine)
            {
                AppendMultiple(' ', Options.spacesInEmptyCollection);
            }
            else if (oneElementPerLine && Options.lineBreaksAfterOpeningBrace > 0)
            {
                AppendMultiple('\n', Options.lineBreaksAfterOpeningBrace);
            }
            else if (Options.spacesAfterOpeningBrace > 0)
            {
                AppendMultiple(' ', Options.spacesAfterOpeningBrace);
            }

            PushIndent();
            IncIndent();

            bool first = true;
            var keys = new List<string>(c.Keys);
            if (Options.sortMapKeys) keys.Sort(string.Compare);
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                var e = c[key];
                PushIndent();

                if (first && (!oneElementPerLine || Options.lineBreaksAfterOpeningBrace == 0))
                {
                    ClearIndent();
                }
                else if (!first)
                {
                    if (oneElementPerLine)
                    {
                        builder.Append('\n');
                    }
                    else
                    {
                        AppendMultiple(' ', Options.spacesAfterComma);
                    }

                    if (!oneElementPerLine)
                        ClearIndent();
                }

                bool hasNext1 = (i + 1) < c.Count;

                FormatComment(e);
                AppendIndent();
                if (Options.alwaysUseStringLiterals || !Utility.IsValidIdentifier(key))
                    key = Utility.EscapeString(key);
                builder.Append(key);
                AppendMultiple(' ', Options.spacesBeforeEquals);
                builder.Append(Options.useJsonDelimiters ? ':' : '=');
                AppendMultiple(' ', Options.spacesAfterEquals);
                FormatElement(e, hasNext1);
                if (hasNext1 && (!e.IsCollection || !Options.omitCommaAfterClosingBrace))
                {
                    AppendMultiple(' ', Options.spacesBeforeComma);
                    builder.Append(',');
                }

                first = false;
                PopIndent();
            }

            if (c.HasTrailingComment && Options.writeComments)
                FormatComment(c.TrailingComment);

            PopIndent();
            if (c.Count != 0 || oneElementPerLine) // Done on the open side
            {
                if (oneElementPerLine && Options.lineBreaksBeforeClosingBrace > 0)
                {
                    AppendMultiple('\n', Options.lineBreaksBeforeClosingBrace);
                    AppendIndent();
                }
                else if (Options.spacesBeforeClosingBrace > 0)
                {
                    AppendMultiple(' ', Options.spacesBeforeClosingBrace);
                }
            }

            builder.Append('}');
            if (!hasNext0 || Options.omitCommaAfterClosingBrace)
            {
                if (oneElementPerLine && Options.lineBreaksAfterClosingBrace > 0)
                {
                    AppendMultiple('\n', Options.lineBreaksAfterClosingBrace);
                }
                else if (Options.spacesAfterClosingBrace > 0)
                {
                    AppendMultiple(' ', Options.spacesAfterClosingBrace);
                }
            }

            PopIndent();
        }

        protected void FormatList(GddlList c, bool hasNext0)
        {
            PushIndent();

            bool oneElementPerLine = !c.IsSimple || c.Count > Options.oneElementPerLineThreshold;

            if (oneElementPerLine && Options.lineBreaksBeforeOpeningBrace > 0)
            {
                AppendMultiple('\n', Options.lineBreaksBeforeOpeningBrace);
                AppendIndent();
            }
            else if (Options.spacesBeforeOpeningBrace > 0)
            {
                AppendMultiple(' ', Options.spacesBeforeOpeningBrace);
            }

            builder.Append('[');
            if (oneElementPerLine && Options.lineBreaksAfterOpeningBrace > 0)
            {
                AppendMultiple('\n', Options.lineBreaksAfterOpeningBrace);
            }
            else if (Options.spacesAfterOpeningBrace > 0)
            {
                AppendMultiple(' ', Options.spacesAfterOpeningBrace);
            }

            PushIndent();
            IncIndent();

            bool first = true;
            for (int i = 0; i < c.Count; i++)
            {
                GddlElement e = c[i];
                PushIndent();

                if (first && (!oneElementPerLine || Options.lineBreaksAfterOpeningBrace == 0))
                {
                    ClearIndent();
                }
                else if (!first)
                {
                    if (oneElementPerLine)
                    {
                        builder.Append('\n');
                    }
                    else if (Options.spacesAfterComma > 0)
                    {
                        AppendMultiple(' ', Options.spacesAfterComma);
                    }

                    if (!oneElementPerLine)
                        ClearIndent();
                }

                bool hasNext1 = (i + 1) < c.Count;

                FormatComment(e);
                AppendIndent();
                FormatElement(e, hasNext1);

                if (hasNext1 && (!e.IsCollection || !Options.omitCommaAfterClosingBrace))
                {
                    AppendMultiple(' ', Options.spacesBeforeComma);
                    builder.Append(',');
                }

                first = false;
                PopIndent();
            }

            if (c.HasTrailingComment && Options.writeComments)
                FormatComment(c.TrailingComment);

            PopIndent();
            if (oneElementPerLine && Options.lineBreaksBeforeClosingBrace > 0)
            {
                AppendMultiple('\n', Options.lineBreaksBeforeClosingBrace);
                AppendIndent();
            }
            else if (Options.spacesBeforeClosingBrace > 0)
            {
                AppendMultiple(' ', Options.spacesBeforeClosingBrace);
            }

            builder.Append(']');
            if (!hasNext0 || Options.omitCommaAfterClosingBrace)
            {
                if (oneElementPerLine && Options.lineBreaksAfterClosingBrace > 0)
                {
                    AppendMultiple('\n', Options.lineBreaksAfterClosingBrace);
                }
                else if (Options.spacesAfterClosingBrace > 0)
                {
                    AppendMultiple(' ', Options.spacesAfterClosingBrace);
                }
            }

            PopIndent();
        }

        #endregion
    }
}