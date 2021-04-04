using System;
using System.Collections.Generic;
using System.Linq;

namespace GDDL.Structure
{
    public class Reference : Element, IEquatable<Reference>
    {
        // Factory Methods
        public static Reference Absolute(params string[] parts)
        {
            return new Reference(true, parts);
        }

        public static Reference Relative(params string[] parts)
        {
            return new Reference(false, parts);
        }

        // Implementation
        protected readonly List<string> nameParts = new List<string>();

        private bool resolved;
        private Element resolvedValue;

        public bool Rooted { get; }

        public override bool IsResolved => resolved;
        public override Element ResolvedValue => resolvedValue;

        public IList<string> NameParts => nameParts.AsReadOnly();

        private Reference(bool rooted, params string[] parts)
        {
            Rooted = rooted;
            nameParts.AddRange(parts);
        }

        public void Add(string name)
        {
            nameParts.Add(name);
        }

        public void AddRange(IEnumerable<string> names)
        {
            nameParts.AddRange(names);
        }

        public override Element Copy()
        {
            return CopyReference();
        }

        public Reference CopyReference()
        {
            var b = new Reference(Rooted);
            CopyTo(b);
            return b;
        }

        protected override void CopyTo(Element other)
        {
            base.CopyTo(other);
            if (!(other is Reference))
                throw new ArgumentException("CopyTo for invalid type", nameof(other));
            var b = (Reference)other;
            b.AddRange(nameParts);
            if (resolved)
            {
                b.resolved = true;
                b.resolvedValue = resolvedValue;
            }
        }

        public override void Resolve(Element root, Element parent)
        {
            if (IsResolved)
                return;

            if (!Rooted && TryResolve(root, parent, true))
            {
                resolved = true;
                return;
            }

            resolved = TryResolve(root, parent, false);
        }

        private bool TryResolve(Element root, Element parent, bool relative)
        {
            var elm = relative ? parent : root;

            bool parentRoot = parent.HasName && nameParts[0] == parent.Name;

            for (int i = parentRoot ? 1 : 0; i < nameParts.Count; i++)
            {
                string part = nameParts[i];

                if (!(elm is Collection s))
                    continue;

                if (s.TryGetValue(part, out var ne))
                {
                    elm = ne;
                    continue;
                }

                resolvedValue = null;
                return false;
            }

            if (!elm.IsResolved)
                elm.Resolve(root, parent);

            resolvedValue = elm.ResolvedValue;

            return resolvedValue != null;
        }

        public override Element Simplify()
        {
            if (!resolved || resolvedValue == null)
                return this;

            var copy = resolvedValue.Copy();
            copy.Name = Name;
            return copy;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            return obj is Reference other && EqualsImpl(other);
        }

        public bool Equals(Reference other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        protected bool EqualsImpl(Reference other)
        {
            if (!base.EqualsImpl(other)) return false;
            return IsResolved == other.IsResolved &&
                Rooted == other.Rooted &&
                Enumerable.SequenceEqual(nameParts, other.nameParts) &&
                Equals(ResolvedValue, other.ResolvedValue);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), nameParts, IsResolved, ResolvedValue, Rooted);
        }
    }
}