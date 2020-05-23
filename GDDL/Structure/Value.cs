using System;
using System.Globalization;
using System.Text;
using GDDL.Config;

namespace GDDL.Structure
{
    public class Value : Element
    {
        public object Data { get; private set; }

        public string String
        {
            get => (string)Data;
            set => Data = value;
        }

        public bool Boolean
        {
            get => (bool)Data;
            set => Data = value;
        }

        public long Integer
        {
            get => (long)Data;
            set => Data = value;
        }

        public double Double
        {
            get => (double)Data;
            set => Data = value;
        }

        internal Value()
        {
            Data = null;
        }

        internal Value(bool valueData)
        {
            Data = valueData;
        }

        internal Value(string valueData)
        {
            Data = valueData;
        }

        internal Value(long valueData)
        {
            Data = valueData;
        }

        internal Value(double valueData)
        {
            Data = valueData;
        }

        public bool IsNull()
        {
            return Data == null;
        }

        public override Element Copy()
        {
            var b = new Value();
            CopyTo(b);
            return b;
        }

        protected override void CopyTo(Element other)
        {
            base.CopyTo(other);
            if (!(other is Value))
                throw new ArgumentException("CopyTo for invalid type");
            var b = (Value)other;
            b.Data = Data;
        }

        protected override void ToStringImpl(StringBuilder builder, StringGenerationContext ctx)
        {
            if (Data == null)
            {
                builder.Append("null");
            }
            else if (Data is bool)
            {
                builder.Append(Boolean ? "true" : "false");
            }
            else if (Data is string)
            {
                builder.Append(Lexer.EscapeString(String));
            }
            else
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}", Data));
            }
        }
    }
}
