using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GDDL.Structure
{
    public sealed class Reference : Element<Reference>, IEquatable<Reference>
    {
        #region Factory Methods

        /**
         * Constructs an absolute reference to the given path.
         * @param parts The target, as an array of names of each element along the path
         * @return A Reference set to the given path
         */
        public static Reference Absolute(params string[] parts)
        {
            return new Reference(true, parts);
        }

        /**
         * Constructs a relative reference to the given path.
         * @param parts The target, as an array of names of each element along the path
         * @return A Reference set to the given path
         */
        public static Reference Relative(params string[] parts)
        {
            return new Reference(false, parts);
        }
        #endregion

        #region Implementation
        private readonly List<string> nameParts = new List<string>();

        private bool resolved;
        private Element resolvedValue;

        public bool Rooted { get; }

        public override bool IsResolved => resolved;
        public override Element ResolvedValue => resolvedValue;

        /**
         * @return The current path of this reference
         */
        public IList<string> NameParts => nameParts.AsReadOnly();

        private Reference(bool rooted, params string[] parts)
        {
            Rooted = rooted;
            nameParts.AddRange(parts);
        }

        /**
         * Adds a new name to the path this Reference represents
         * @param name The name of a named element
         */
        public void Add(string name)
        {
            nameParts.Add(name);
        }

        /**
         * Appends the given collection of names to the path this Reference represents
         * @param names The collection of names
         */
        public void AddRange(IEnumerable<string> names)
        {
            nameParts.AddRange(names);
        }
        #endregion

        #region Element

        public override Reference CopyInternal()
        {
            var reference = new Reference(Rooted);
            CopyTo(reference);
            return reference;
        }

        protected override void CopyTo(Reference other)
        {
            base.CopyTo(other);
            other.AddRange(nameParts);
        }

        public override void Resolve(Element root, [MaybeNull] Collection parent)
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

        private bool TryResolve(Element root, [MaybeNull] Collection parent, bool relative)
        {
            var target = relative ? (Element)parent ?? this : root;

            bool parentRoot = target.HasName && nameParts[0] == target.Name;

            for (int i = parentRoot ? 1 : 0; i < nameParts.Count; i++)
            {
                string part = nameParts[i];

                if (!(target is Collection s))
                    continue;

                if (s.TryGetValue(part, out var ne))
                {
                    target = ne;
                    continue;
                }

                resolvedValue = null;
                return false;
            }

            if (!target.IsResolved)
                target.Resolve(root, target.ParentInternal);

            resolvedValue = target.ResolvedValue;

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
        #endregion

        #region Equality

        public override bool Equals(object other)
        {
            if (other == this) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((Reference)other);
        }

        public override bool Equals(Reference other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(Reference other)
        {
            return base.EqualsImpl(other) &&
                Rooted == other.Rooted &&
                Enumerable.SequenceEqual(nameParts, other.nameParts) &&
                (IsResolved
                    ? other.IsResolved && Equals(ResolvedValue, other.ResolvedValue)
                    : !other.IsResolved);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), nameParts, IsResolved, ResolvedValue, Rooted);
        }
        #endregion
    }
}