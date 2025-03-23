using GDDL.Util;
using System;

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

        public void SetString(string value)
        {
            data = Utility.RequireNotNull(value);
        }

        public void SetInt(long value)
        {
            data = value;
        }

        public void SetDouble(double value)
        {
            data = value;
        }

        public void SetBool(bool value)
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

        #region IConvertible
        public override TypeCode GetTypeCode()
        {
            return data is IConvertible ic ? ic.GetTypeCode() : TypeCode.Object;
        }
        public override bool ToBoolean(IFormatProvider provider) => AsBoolean;
        public override byte ToByte(IFormatProvider provider) => IsString ? Convert.ToByte(AsString, provider) : (byte)AsInteger;
        public override sbyte ToSByte(IFormatProvider provider) => IsString ? Convert.ToSByte(AsString, provider) : (sbyte)AsInteger;
        public override short ToInt16(IFormatProvider provider) => IsString ? Convert.ToInt16(AsString, provider) : (short)AsInteger;
        public override ushort ToUInt16(IFormatProvider provider) => IsString ? Convert.ToUInt16(AsString, provider) : (ushort)AsInteger;
        public override int ToInt32(IFormatProvider provider) => IsString ? Convert.ToInt32(AsString, provider) : (int)AsInteger;
        public override uint ToUInt32(IFormatProvider provider) => IsString ? Convert.ToUInt32(AsString, provider) : (uint)AsInteger;
        public override long ToInt64(IFormatProvider provider) => IsString ? Convert.ToInt64(AsString, provider) : AsInteger;
        public override ulong ToUInt64(IFormatProvider provider) => IsString ? Convert.ToUInt64(AsString, provider) : (ulong)AsInteger;
        public override float ToSingle(IFormatProvider provider) => IsString ? Convert.ToSingle(AsString, provider) : (float)AsDouble;
        public override double ToDouble(IFormatProvider provider) => IsString ? Convert.ToDouble(AsString, provider) : AsDouble;
        public override decimal ToDecimal(IFormatProvider provider) => IsString ? Convert.ToDecimal(AsString, provider) : (IsInteger ? (decimal)AsInteger : (decimal)AsDouble);
        public override char ToChar(IFormatProvider provider) => (char)(IsString ? Convert.ToInt32(AsString, provider) : AsInteger);
        public override string ToString(IFormatProvider provider) => AsString;
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

        protected override GddlValue CopyInternal()
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

        #region Implicit Casts
        public static implicit operator GddlValue(byte b) { return Of(b); }
        public static implicit operator GddlValue(sbyte b) { return Of(b); }
        public static implicit operator GddlValue(short b) { return Of(b); }
        public static implicit operator GddlValue(ushort b) { return Of(b); }
        public static implicit operator GddlValue(int b) { return Of(b); }
        public static implicit operator GddlValue(uint b) { return Of(b); }
        public static implicit operator GddlValue(long b) { return Of(b); }
        public static implicit operator GddlValue(ulong b) { return Of(b); }
        public static implicit operator GddlValue(float b) { return Of(b); }
        public static implicit operator GddlValue(double b) { return Of(b); }
        #endregion
    }
}