using GDDL.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GDDL.Structure
{
    public abstract class GddlElement
    {
        protected internal GddlElement()
        {
        }
        
        public string Comment { get; internal set; }
        
        public virtual bool IsResolved => true;
        public virtual GddlElement ResolvedValue => this;
        
        public bool HasComment => !string.IsNullOrEmpty(Comment);

        public bool IsList => this is GddlList;
        public GddlList AsList => (GddlList)this;

        public bool IsMap => this is GddlMap;
        public GddlList AsMap => (GddlList)this;

        public bool IsValue => this is GddlValue;
        public GddlValue AsValue => (GddlValue)this;

        public virtual GddlElement Simplify()
        {
            return this;
        }

        public virtual void Resolve(GddlElement root)
        {
        }

        public sealed override string ToString()
        {
            return Formatter.FormatCompact(this);
        }
        
        public override abstract bool Equals(object obj);

        protected bool EqualsImpl(GddlElement other)
        {
            return (string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(other.Comment)) || Equals(Comment, other.Comment);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Comment);
        }

        public GddlElement Copy()
        {
            return CopyBridge();
        }

        protected internal abstract GddlElement CopyBridge();
    }

    public abstract class Element<T> : GddlElement, IEquatable<T>
        where T : Element<T>
    {
        internal protected Element()
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

        protected abstract void CopyTo(T other);

        public abstract bool Equals(T other);
    }
}