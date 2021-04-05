using System;
using GDDL.Util;

namespace GDDL.Structure
{
    /**
     * Represents a simple value within the GDDL hierarchy.
     * Simple values are: null, boolean false and true, strings, integers (long), and floats (double).
     */
    public sealed class Value : Element<Value>, IEquatable<Value>
    {
        // Factory Methods

        /**
         * Constructs a Value representing `null`
         * @return The value
         */
        public static Value Null()
        {
            return new Value();
        }

        /**
         * Constructs a Value representing the given boolean
         * @return The value
         */
        public static Value Of(bool value)
        {
            return new Value(value);
        }

        /**
         * Constructs a Value representing the given long integer
         * @return The value
         */
        public static Value Of(long num)
        {
            return new Value(num);
        }

        /**
         * Constructs a Value representing the given floating-point number
         * @return The value
         */
        public static Value Of(double num)
        {
            return new Value(num);
        }

        /**
         * Constructs a Value representing the given string
         * @return The value
         */
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

        private Value()
        {
            data = null;
        }

        private Value(bool valueData)
        {
            data = valueData;
        }

        private Value(string valueData)
        {
            data = valueData;
        }

        private Value(long valueData)
        {
            data = valueData;
        }

        private Value(double valueData)
        {
            data = valueData;
        }

        /**
         * Changes the contained value to be `null`
         */
        public void SetNull()
        {
            data = null;
        }

        public override Value CopyInternal()
        {
            var value = new Value();
            CopyTo(value);
            return value;
        }

        protected override void CopyTo(Value other)
        {
            base.CopyTo(other);
            other.data = data;
        }

        public override bool Equals(object other)
        {
            if (other == this) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((Value)other);
        }

        public override bool Equals(Value other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(Value other)
        {
            return base.EqualsImpl(other) && Equals(data, other.data);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), data);
        }
    }
}
