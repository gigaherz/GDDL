using System;
using GDDL.Util;

namespace GDDL.Structure
{
    public class Value : Element, IEquatable<Value>
    {
        // Factory Methods
        public static Value Null()
        {
            return new Value();
        }

        public static Value Of(bool value)
        {
            return new Value(value);
        }

        public static Value Of(long num)
        {
            return new Value(num);
        }

        public static Value Of(double num)
        {
            return new Value(num);
        }

        public static Value Of(string s)
        {
            return new Value(s ?? "");
        }

        // Implementation
        private object data;

        public bool IsNull => data == null;
        public bool IsBoolean => data is bool;
        public bool IsInteger => data is long;
        public bool IsDouble => data is double;
        public bool IsString => data is string;

        public string String
        {
            get => (string)Utility.RequireNotNull(data);
            set => data = Utility.RequireNotNull(value);
        }

        public bool Boolean
        {
            get => (bool)data;
            set => data = value;
        }

        public long Integer
        {
            get => (long)data;
            set => data = value;
        }

        public double Double
        {
            get => (double)data;
            set => data = value;
        }

        internal Value()
        {
            data = null;
        }

        internal Value(bool valueData)
        {
            data = valueData;
        }

        internal Value(string valueData)
        {
            data = valueData;
        }

        internal Value(long valueData)
        {
            data = valueData;
        }

        internal Value(double valueData)
        {
            data = valueData;
        }

        public void SetNull()
        {
            data = null;
        }

        public override Element Copy()
        {
            return CopyValue();
        }

        public Value CopyValue()
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
            b.data = data;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            return obj is Value other ? EqualsImpl(other) : false;
        }

        public bool Equals(Value other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        protected bool EqualsImpl(Value other)
        {
            if (!base.EqualsImpl(other)) return false;
            return Equals(data, other.data);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), data);
        }
    }
}
