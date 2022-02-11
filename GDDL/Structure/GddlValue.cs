using System;
using GDDL.Util;

namespace GDDL.Structure
{
    /**
     * Represents a simple value within the GDDL hierarchy.
     * Simple values are: null, boolean false and true, strings, integers (long), and floats (double).
     */
    public sealed class GddlValue : GddlElement<GddlValue>, IEquatable<GddlValue>
    {
        #region API

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

        public override bool IsValue => true;
        public override GddlValue AsValue => this;

        public override bool IsNull => data == null;

        public override bool IsBoolean => data is bool;
        public override bool AsBoolean => (bool)data;

        public override bool IsInteger => data is long;
        public override long AsInteger => (long)data;

        public override bool IsDouble => data is double;
        public override double AsDouble => (double)data;

        public override bool IsString => data is string;
        public override string AsString => (string)Utility.RequireNotNull(data);

        public void Set(string value)
        {
            data = Utility.RequireNotNull(value);
        }

        public void Set(int value)
        {
            data = value;
        }

        public void Set(double value)
        {
            data = value;
        }

        public void Set(bool value)
        {
            data = value;
        }

        /**
         * Changes the contained value to be `null`
         */
        public void SetNull()
        {
            data = null;
        }
        #endregion

        #region Implementation
        private object data;

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
            if (ReferenceEquals(other, this)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((GddlValue)other);
        }

        public override bool Equals(GddlValue other)
        {
            if (ReferenceEquals(other, this)) return true;
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
