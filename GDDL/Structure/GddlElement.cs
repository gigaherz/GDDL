//#define DYNAMIC

using GDDL.Serialization;
using System;
using System.Collections.Generic;
#if DYNAMIC
using System.Dynamic;
using System.Linq;
#endif
using GDDL.Util;

namespace GDDL.Structure
{
    public abstract class GddlElement : IConvertible
#if DYNAMIC
        : DynamicObject
#endif
    {
        #region API

#if DYNAMIC
        public dynamic Dynamic => this;
#endif

        public string Comment { get; set; }
        public bool HasComment => !string.IsNullOrEmpty(Comment);

        public string Whitespace { get; set; }
        public bool HasWhitespace => !string.IsNullOrEmpty(Whitespace);

        public virtual bool IsResolved => true;
        public virtual GddlElement ResolvedValue => this;

        public virtual bool IsMap => false;
        public virtual GddlMap AsMap => throw new InvalidCastException("This element is not a Map.");

        public virtual bool IsList => false;
        public virtual GddlList AsList => throw new InvalidCastException("This element is not a List.");

        public virtual bool IsValue => false;
        public virtual GddlValue AsValue => throw new InvalidCastException("This element is not a Value.");

        public virtual bool IsReference => false;
        public virtual GddlReference AsReference => throw new InvalidCastException("This element is not a Reference.");

        public virtual bool IsCollection => IsMap || IsList;

        public virtual bool IsNull => false;

        public virtual bool IsBoolean => false;
        public virtual bool AsBoolean => throw new InvalidCastException("This element is not a Value.");

        public virtual bool IsInteger => false;
        public virtual long AsInteger => throw new InvalidCastException("This element is not a Value.");

        public virtual bool IsDouble => false;
        public virtual double AsDouble => throw new InvalidCastException("This element is not a Value.");

        public virtual bool IsString => false;
        public virtual string AsString => throw new InvalidCastException("This element is not a Value.");

        public GddlElement Parent { get; protected internal set; }

        public virtual GddlElement this[int index]
        {
            get => throw new InvalidCastException("This element is not a List.");
            set => throw new InvalidCastException("This element is not a List.");
        }

        public virtual GddlElement this[Index index]
        {
            get => throw new InvalidCastException("This element is not a List.");
            set => throw new InvalidCastException("This element is not a List.");
        }

        public virtual SubList<GddlElement> this[Range range]
        {
            get => throw new InvalidCastException("This element is not a List.");
            set => throw new InvalidCastException("This element is not a List.");
        }

        public virtual GddlElement this[string key]
        {
            get => throw new InvalidCastException("This element is not a Map.");
            set => throw new InvalidCastException("This element is not a Map.");
        }

        public virtual GddlElement Simplify()
        {
            return this;
        }

        public virtual void Resolve(GddlElement root)
        {
        }

        public IEnumerable<GddlElement> Query(string query)
        {
            return Queries.Query.FromString(query).Apply(this);
        }

        public GddlElement Copy()
        {
            return CopyBridge();
        }

        public virtual int GetFormattingComplexity()
        {
            return 1;
        }

        #endregion

#if DYNAMIC
#region Dynamic

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return IsMap ? Enumerable.Empty<string>() : AsMap.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (IsMap)
            {
                result = AsMap[binder.Name];
                return true;
            }

            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (IsMap)
            {
                AsMap[binder.Name] = (GddlElement)value;
                return true;
            }

            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length == 1 && IsList && indexes[0] is int i)
            {
                result = AsList[i];
                return true;
            }
            else if (indexes.Length == 1 && IsMap && indexes[0] is string s)
            {
                result = AsMap[s];
                return true;
            }
            result = null;
            return false;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length == 1 && IsList && indexes[0] is int i)
            {
                AsList[i] = (GddlElement)value;
                return true;
            }
            else if (indexes.Length == 1 && IsMap && indexes[0] is string s)
            {
                AsMap[s] = (GddlElement)value;
                return true;
            }
            return false;
        }

#endregion
#endif

        #region ToString

        public sealed override string ToString()
        {
            return Formatter.FormatCompact(this);
        }

        #endregion

        #region Equality

        public abstract override bool Equals(object obj);

        protected bool EqualsImpl(GddlElement other)
        {
            return !(HasComment || other.HasComment) || Equals(Comment, other.Comment);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Comment);
        }

        #endregion

        #region IConvertible
        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => throw new NotImplementedException("Cannot convert this way, use a GddlSerializer.");
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new InvalidCastException("Cannot convert to date.");
        public virtual TypeCode GetTypeCode() => TypeCode.Object;
        public virtual bool ToBoolean(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual byte ToByte(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual sbyte ToSByte(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual short ToInt16(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual ushort ToUInt16(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual int ToInt32(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual uint ToUInt32(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual long ToInt64(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual ulong ToUInt64(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual float ToSingle(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual double ToDouble(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual decimal ToDecimal(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual char ToChar(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        public virtual string ToString(IFormatProvider provider) => throw new InvalidCastException("This element is not a Value.");
        #endregion

        #region Implementation

        protected internal GddlElement()
        {
        }

        protected internal abstract GddlElement CopyBridge();

        #endregion
    }

    public abstract class GddlElement<T> : GddlElement, IEquatable<T>
        where T : GddlElement<T>
    {
        #region API

        public T WithComment(string comment)
        {
            Comment = comment;
            return (T)this;
        }

        #endregion

        #region Implementation

        protected internal GddlElement()
        {
        }

        protected internal sealed override GddlElement CopyBridge()
        {
            return Copy();
        }

        public new T Copy()
        {
            var copy = CopyInternal();
            copy.Resolve(this);
            return copy;
        }

        protected abstract T CopyInternal();

        protected virtual void CopyTo(T other)
        {
            if (HasWhitespace)
                other.Whitespace = Whitespace;
            if (HasComment)
                other.Comment = Comment;
        }

        public abstract bool Equals(T other);

        #endregion
    }
}