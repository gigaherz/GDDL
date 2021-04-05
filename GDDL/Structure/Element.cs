using GDDL.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GDDL.Structure
{
    public abstract class Element
    {
        internal protected Element()
        {
        }

        [MaybeNull]
        internal protected Collection ParentInternal { get; set; }

        public string Comment { get; internal set; }

        private string name;
        public string Name {
            get => name;
            internal set
            {
                if (ParentInternal != null)
                    ParentInternal.SetName(this, value);
                else
                    SetNameInternal(value);
            } 
        }
        
        internal protected void SetNameInternal(String name)
        {
            this.name = name;
        }

        public virtual bool IsResolved => true;
        public virtual Element ResolvedValue => this;

        public bool HasName => !string.IsNullOrEmpty(Name);

        public bool HasComment => !string.IsNullOrEmpty(Comment);

        public bool IsCollection => this is Collection;
        public Collection AsCollection => (Collection)this;

        public bool IsValue => this is Value;
        public Value AsValue => (Value)this;

        public virtual Element Simplify()
        {
            return this;
        }

        public virtual void Resolve(Element root, [MaybeNull] Collection parent)
        {
        }

        public sealed override string ToString()
        {
            return Formatter.FormatCompact(this);
        }

        public Element WithName(string name)
        {
            this.Name = name;
            return this;
        }

        public override abstract bool Equals(object obj);

        protected bool EqualsImpl(Element other)
        {
            return ((string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(other.Comment)) || Equals(Comment, other.Comment)) &&
                    Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Comment, Name);
        }

        public Element Copy()
        {
            return CopyBridge();
        }

        protected internal abstract Element CopyBridge();
    }

    public abstract class Element<T> : Element, IEquatable<T>
        where T : Element<T>
    {
        internal protected Element()
        {
        }

        protected internal sealed override Element CopyBridge()
        {
            return Copy();
        }

        public new T Copy()
        {
            T copy = CopyInternal();
            copy.Resolve(this, null);
            return copy;
        }

        public abstract T CopyInternal();

        protected virtual void CopyTo(T other)
        {
            if (HasName)
                other.Name = Name;
        }

        public new T WithName(string name)
        {
            return (T)base.WithName(name);
        }

        public abstract bool Equals(T other);
    }
}