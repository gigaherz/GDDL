using System;
using GDDL.Util;

namespace GDDL.Structure
{
    /**
     * Represents a simple value within the GDDL hierarchy.
     * Simple values are: null, boolean false and true, strings, integers (long), and floats (double).
     */
    public sealed class GddlValue : Element<GddlValue>, IEquatable<GddlValue>
    {
        #region Factory Methods

        /**
         * Constructs a Value representing `null`
         * @return The value
         */
        public static GddlValue Null()
        {
            return new GddlValue();
        }

        /**
         * Constructs a Value representing the given boolean
         * @return The value
         */
        public static GddlValue Of(bool value)
        {
            return new GddlValue(value);
        }

        /**
         * Constructs a Value representing the given long integer
         * @return The value
         */
        public static GddlValue Of(long num)
        {
            return new GddlValue(num);
        }

        /**
         * Constructs a Value representing the given floating-point number
         * @return The value
         */
        public static GddlValue Of(double num)
        {
            return new GddlValue(num);
        }

        /**
         * Constructs a Value representing the given string
         * @return The value
         */
        public static GddlValue Of(string s)
        {
            return new GddlValue(s ?? "");
        }
        #endregion

        #region Implementation
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

        private GddlValue()
        {
            data = null;
        }

        private GddlValue(bool valueData)
        {
            data = valueData;
        }

        private GddlValue(string valueData)
        {
            data = valueData;
        }

        private GddlValue(long valueData)
        {
            data = valueData;
        }

        private GddlValue(double valueData)
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
        #endregion

        #region Element

        public override GddlValue CopyInternal()
        {
            var value = new GddlValue();
            CopyTo(value);
            return value;
        }

        protected override void CopyTo(GddlValue other)
        {
            base.CopyTo(other);
            other.data = data;
        }
        #endregion

        #region Equals

        public override bool Equals(object other)
        {
            if (other == this) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((GddlValue)other);
        }

        public override bool Equals(GddlValue other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(GddlValue other)
        {
            return base.EqualsImpl(other) && Equals(data, other.data);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), data);
        }
        #endregion
    }
}
