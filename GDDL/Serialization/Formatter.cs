using GDDL.Structure;
using GDDL.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GDDL.Serialization
{
    public partial class Formatter
    {
        public static string FormatCompact(Element e)
        {
            return Format(e, FormatterOptions.Compact);
        }

        public static string FormatNice(Element e)
        {
            return Format(e, FormatterOptions.Nice);
        }

        public static string Format(Element e, FormatterOptions options)
        {
            var b = new StringBuilder();
            var f = new Formatter(b, options);
            f.FormatStandalone(e);
            return b.ToString();
        }

        private readonly Stack<int> IndentLevels = new Stack<int>();
        private readonly FormatterOptions options;
        private readonly StringBuilder builder;

        public int IndentLevel = 0;

        public Formatter(StringBuilder builder, FormatterOptions options)
        {
            this.builder = builder;
            this.options = options;
        }

        public void PushIndent()
        {
            IndentLevels.Push(IndentLevel);
        }

        public void PopIndent()
        {
            IndentLevel = IndentLevels.Pop();
        }

        public void SetIndent(int newIndent)
        {
            IndentLevel = newIndent;
        }

        public void IncIndent()
        {
            IndentLevel++;
        }

        private void AppendMultiple(string s, int n)
        {
            for (int i = 0; i < n; i++)
            {
                builder.Append(s);
            }
        }

        public void AppendIndent()
        {
            int tabsToGen = IndentLevel;
            for (int i = 0; i < tabsToGen; i++)
            {
                if (options.indentUsingTabs)
                {
                    builder.Append("\t");
                }
                else
                {
                    for (int j = 0; j < options.spacesPerIndent; j++)
                    {
                        builder.Append(" ");
                    }
                }
            }
        }

        public void FormatStandalone(Element e)
        {
            FormatComment(e);
            FormatName(e);
            FormatElement(e, false);
        }

        private static Regex CommentLineSplitter = new Regex("(?:(?:\n)|(?:\r\n))");

        protected void FormatComment(Element e)
        {
            if (e.HasComment && options.writeComments)
            {
                for (int i = 0; i < options.blankLinesBeforeComment; i++)
                    builder.Append("\n");
                foreach (var s in CommentLineSplitter.Split(e.Comment))
                {
                    AppendIndent();
                    builder.Append("#");
                    builder.Append(s);
                    builder.Append("\n");
                }
            }
        }

        protected void FormatName(Element e)
        {
            AppendIndent();
            if (e.HasName)
            {
                string sname = e.Name;
                if (!Utility.IsValidIdentifier(sname))
                    sname = Utility.EscapeString(sname);
                builder.Append(sname);
                builder.Append(" = ");
            }
        }

        protected void FormatElement(Element e, bool hasNext)
        {
            if (e is Value v)
            {
                FormatValue(v);
            }
            else if (e is Reference r)
            {
                FormatReference(r);
            }
            else if (e is Collection c)
            {
                FormatCollection(c, hasNext);
            }
            else
            {
                throw new NotImplementedException("A new Element type has been added without updating Formatter#FormatElement.");
            }
        }

        protected void FormatValue(Value v)
        {
            if (v.IsNull)
            {
                builder.Append("null");
            }
            else if (v.IsBoolean)
            {
                builder.Append(v.Boolean ? "true" : "false");
            }
            else if (v.IsInteger)
            {
                FormatInteger(v.Integer);
            }
            else if (v.IsDouble)
            {
                FormatDoubleCustom(v.Double);
            }
            else if (v.IsString)
            {
                builder.Append(Utility.EscapeString(v.String));
            }
            else
            {
                throw new NotImplementedException("A new Value type has been added without updating Formatter#formatValue.");
            }
        }

        protected void FormatInteger(long value)
        {
            builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}", value));
        }

        protected void FormatDoubleCustom(double value)
        {
            switch (options.floatFormattingStyle)
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
            if (exp >= options.autoScientificNotationUpper || exp < options.autoScientificNotationLower)
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
            builder.Append("e");
            if (options.alwaysShowExponentSign)
                FormatSign(exp);
            else
                FormatNegative(exp);
            FormatInteger(Math.Abs(exp));
        }

        protected void FormatDoubleDecimal(double value)
        {
            if (options.alwaysShowNumberSign)
                FormatSign(value);
            else
                FormatNegative(value);
            value = Math.Abs(value);

            double integral = Math.Floor(value);
            double fractional = value - integral;

            var temp = new List<int>();

            int intDigits = FormatIntegral(integral, temp);

            builder.Append(".");

            FormatFractional(fractional, intDigits, temp);
        }

        private int FormatIntegral(double integral, List<int> temp)
        {
            if (!(integral > 0))
            {
                builder.Append('0');
                return 0;
            }

            int exp = (int)Math.Ceiling(Math.Log10(integral));
            double value = integral / Math.Pow(10, exp);

            int nonTrailingDigits = FormatDigits(temp, Math.Min(exp, options.floatSignificantFigures), value);
            for (int i = nonTrailingDigits; i < exp; i++)
            {
                builder.Append('0');
            }
            return exp;
        }

        private void FormatFractional(double fractional, int intDigits, List<int> temp)
        {
            FormatDigits(temp, (options.floatSignificantFigures - intDigits), fractional);
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

        private int RoundDigits(List<int> temp, double value)
        {
            int l = temp.Count - 1;
            int r = value >= 0.5 ? 1 : 0;
            while (r > 0 && l >= 0) // round up
            {
                int v = temp[l];
                v++;
                if (v >= 10)
                {
                    r = 1;
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
            else if (!double.IsFinite(value))
            {
                if (options.alwaysShowNumberSign)
                    FormatSign(value);
                else
                    FormatNegative(value);
                builder.Append(".Inf");
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FormatNegative(double value)
        {
            long l = BitConverter.DoubleToInt64Bits(value);
            if (l < 0) builder.Append("-");
        }

        private void FormatSign(double value)
        {
            long l = BitConverter.DoubleToInt64Bits(value);
            builder.Append(l < 0 ? "-" : "+");
        }

        protected void FormatReference(Reference r)
        {
            int count = 0;
            foreach (string it in r.NameParts)
            {
                if (count++ > 0)
                    builder.Append(':');
                builder.Append(it);
            }

            if (r.IsResolved)
            {
                builder.Append('=');
                if (r.ResolvedValue == null)
                    builder.Append("NULL");
                else
                    builder.Append(r.ResolvedValue);
            }
        }

        protected void FormatCollection(Collection c, bool hasNext0)
        {
            PushIndent();

            bool oneElementPerLine = !c.IsSimple || c.Count > options.oneElementPerLineThreshold;

            if (c.HasTypeName)
            {
                builder.Append(c.TypeName);
                if (options.lineBreaksBeforeOpeningBrace == 0)
                    builder.Append(" ");
            }
            bool addBraces = IndentLevel > 0 || c.HasTypeName;
            if (addBraces)
            {
                if (oneElementPerLine && options.lineBreaksBeforeOpeningBrace > 0)
                {
                    AppendMultiple("\n", options.lineBreaksBeforeOpeningBrace);
                    AppendIndent();
                }
                else if (options.spacesBeforeOpeningBrace > 0)
                {
                    AppendMultiple(" ", options.spacesBeforeOpeningBrace);
                }
                builder.Append("{");
                if (oneElementPerLine && options.lineBreaksAfterOpeningBrace > 0)
                {
                    AppendMultiple("\n", options.lineBreaksAfterOpeningBrace);
                }
                else if (options.spacesAfterOpeningBrace > 0)
                {
                    AppendMultiple(" ", options.spacesAfterOpeningBrace);
                }
                PushIndent();
                IncIndent();
            }

            bool first = true;
            for (int i = 0; i < c.Count; i++)
            {
                Element e = c[i];
                PushIndent();

                if (first && (!oneElementPerLine || options.lineBreaksAfterOpeningBrace == 0))
                {
                    SetIndent(0);
                }
                else if (!first)
                {
                    if (oneElementPerLine)
                    {
                        builder.Append("\n");
                    }
                    else if (options.spacesBetweenElements > 0)
                    {
                        AppendMultiple(" ", options.spacesBetweenElements);
                    }

                    if (!oneElementPerLine)
                        SetIndent(0);
                }

                bool hasNext1 = (i + 1) < c.Count;
                FormatComment(e);
                FormatName(e);
                FormatElement(e, hasNext1);
                if (hasNext1 && (!e.IsCollection || !options.omitCommaAfterClosingBrace)) builder.Append(",");

                first = false;
                PopIndent();
            }

            if (addBraces)
            {
                PopIndent();
                if (oneElementPerLine && options.lineBreaksBeforeClosingBrace > 0)
                {
                    AppendMultiple("\n", options.lineBreaksBeforeClosingBrace);
                    AppendIndent();
                }
                else if (options.spacesBeforeClosingBrace > 0)
                {
                    AppendMultiple(" ", options.spacesBeforeClosingBrace);
                }
                builder.Append("}");
                if (!hasNext0 || options.omitCommaAfterClosingBrace)
                {
                    if (oneElementPerLine && options.lineBreaksAfterClosingBrace > 0)
                    {
                        AppendMultiple("\n", options.lineBreaksAfterClosingBrace);
                    }
                    else if (options.spacesAfterClosingBrace > 0)
                    {
                        AppendMultiple(" ", options.spacesAfterClosingBrace);
                    }
                }
            }

            PopIndent();
        }
    }
}
