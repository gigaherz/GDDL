using System;
using System.Globalization;
using GDDL.Config;

namespace GDDL.Structure
{
    public class Value : Element
    {
        public object Data { get; private set; }

        public string String
        {
            get
            {
                return (string)Data;
            }

            set
            {
                Data = value;
            }
        }

        public bool Boolean
        {
            get
            {
                return (bool)Data;
            }

            set
            {
                Data = value;
            }
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

        protected override string ToStringInternal(StringGenerationContext ctx)
        {
            if (Data == null)
            {
                return "null";
            }
            if (Data is bool)
            {
                return Boolean ? "true" : "false";
            }
            if (Data is string)
            {
                return Lexer.EscapeString(String);
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}", Data);
        }

    }
}
