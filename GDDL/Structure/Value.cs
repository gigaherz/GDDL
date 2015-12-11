using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace GDDL.Structure
{
    public class Value : Element
    {
        public virtual object Data { get; private set; }

        public virtual string String
        {
            get { return (string)Data; }
            set { Data = value; }
        }

        public virtual double Float
        {
            get { return (double)Data; }
            set { Data = value; }
        }

        public virtual long Integer
        {
            get { return (long)Data; }
            set { Data = value; }
        }

        public virtual bool Boolean
        {
            get { return (bool)Data; }
            set { Data = value; }
        }

        public virtual bool IsNull
        {
            get { return Data == null; }
        }

        internal Value() { Data = null; }

        internal Value(bool valueData) { Data = valueData; }

        internal Value(string valueData) { Data = valueData; }

        internal Value(long valueData) { Data = valueData; }

        internal Value(double valueData) { Data = valueData; }

        internal static string UnescapeString(string p)
        {
            var sb = new StringBuilder();

            bool q = false;
            bool b = false;
            bool u = false;

            int u1 = 0;
            int u2 = 0;

            foreach (var c in p)
            {
                if (q)
                {
                    if (u)
                    {
                        if (u2 == 4)
                        {
                            sb.Append((char)u1);
                            u = false;
                        }
                        else if (char.IsDigit(c))
                        {
                            u1 = u1 * 16 + (c - '0');
                        }
                        else if ((u2 < 4) && ((c >= 'a') && (c <= 'f')))
                        {
                            u1 = u1 * 16 + 10 + (c - 'a');
                        }
                        else if ((u2 < 4) && ((c >= 'A') && (c <= 'F')))
                        {
                            u1 = u1 * 16 + 10 + (c - 'A');
                        }
                        else
                        {
                            sb.Append((char)u1);
                            u = false;
                        }
                        u2++;
                    }

                    if (b)
                    {
                        switch (c)
                        {
                            case '"':
                                sb.Append('"');
                                break;
                            case '\\':
                                sb.Append('\\');
                                break;
                            case 'b':
                                sb.Append('\b');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'f':
                                sb.Append('\f');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 'u':
                            case 'x':
                                u = true;
                                b = false;
                                u1 = 0;
                                u2 = 0;
                                break;
                        }
                    }
                    else
                    {
                        switch (c)
                        {
                            case '"':
                                //q = false;
                                //break;
                                return sb.ToString();
                            case '\\':
                                b = true;
                                break;
                            default:
                                sb.Append(c);
                                break;
                        }
                    }
                }
                else
                {
                    switch (c)
                    {
                        case '"':
                            q = true;
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
            }

            throw new InvalidDataException();
        }

        internal static string EscapeString(string p)
        {
            var sb = new StringBuilder();

            sb.Append('"');
            foreach (var c in p)
            {
                if (!char.IsControl(c) && c != '"' && c != '\\')
                {
                    sb.Append(c);
                    continue;
                }

                sb.Append('\\');
                switch (c)
                {
                    case '\b':
                        sb.Append('b');
                        break;
                    case '\t':
                        sb.Append('t');
                        break;
                    case '\n':
                        sb.Append('n');
                        break;
                    case '\f':
                        sb.Append('f');
                        break;
                    case '\r':
                        sb.Append('r');
                        break;
                    case '\"':
                        sb.Append('\"');
                        break;
                    case '\\':
                        sb.Append('\\');
                        break;
                    default:
                        sb.AppendFormat("u{0:X4}", (int)c);
                        break;
                }
            }
            sb.Append('"');

            return sb.ToString();
        }

        protected override string ToStringInternal()
        {
            if (Data == null)
            {
                return "nil";
            }
            if (Data is bool)
            {
                return Boolean ? "true" : "false";
            }
            if (Data is string)
            {
                return EscapeString(Data as string);
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}", Data);
        }

        protected override string ToStringInternal(StringGenerationContext ctx)
        {
            return ToStringInternal();
        }

        public static explicit operator long(Value v)
        {
            if (v.Data is double)
                return (long)v.Float;
            if (v.Data is long)
                return v.Integer;
            throw new InvalidCastException();
        }

        public static explicit operator double(Value v)
        {
            if (v.Data is double)
                return v.Float;
            if (v.Data is long)
                return v.Integer;
            throw new InvalidCastException();
        }

        public static explicit operator int(Value v)
        {
            if (v.Data is double)
                return (int)v.Float;
            if (v.Data is long)
                return (int)v.Integer;
            throw new InvalidCastException();
        }

        public static explicit operator float(Value v)
        {
            if (v.Data is double)
                return (float)v.Float;
            if (v.Data is long)
                return v.Integer;
            throw new InvalidCastException();
        }

        public static explicit operator string(Value v)
        {
            return v.ToStringInternal();
        }
    }
}