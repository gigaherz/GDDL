using GDDL.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GDDL.Structure
{
    public abstract class GddlElement
    {
        #region API
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

        public virtual GddlElement Simplify()
        {
            return this;
        }

        public virtual void Resolve(GddlElement root)
        {
        }

        public GddlElement Copy()
        {
            return CopyBridge();
        }
        #endregion

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
            return (string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(other.Comment)) || Equals(Comment, other.Comment);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Comment);
        }
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
            T copy = CopyInternal();
            copy.Resolve(this);
            return copy;
        }

        public abstract T CopyInternal();

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