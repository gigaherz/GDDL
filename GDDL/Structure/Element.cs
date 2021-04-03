using GDDL.Serialization;
using System;

namespace GDDL.Structure
{
    public abstract class Element
    {
        public string Comment { get; internal set; }
        public string Name { get; internal set; }

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

        public virtual void Resolve(Element root, Element parent)
        {
        }

        public sealed override string ToString()
        {
            return Formatter.FormatCompact(this);
        }

        public abstract Element Copy();

        protected virtual void CopyTo(Element other)
        {
            if (HasName)
                other.Name = Name;
        }

        public Element WithName(string name)
        {
            this.Name = name;
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            return obj is Element e ? EqualsImpl(e) : false;
        }

        protected bool EqualsImpl(Element other)
        {
            return ((string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(other.Comment)) || Equals(Comment, other.Comment)) &&
                    Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Comment, Name);
        }
    }
}