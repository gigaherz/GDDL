using System;
using System.Collections.Generic;
using System.Linq;
using GDDL.Util;

namespace GDDL.Structure
{
    public sealed class GddlReference : GddlElement<GddlReference>, IEquatable<GddlReference>
    {
        #region API

        /**
         * Constructs an absolute reference to the given path.
         * @param parts The target, as an array of names of each element along the path
         * @return A Reference set to the given path
         */
        public static GddlReference Absolute(params string[] parts)
        {
            return new GddlReference(true, parts);
        }

        /**
         * Constructs a relative reference to the given path.
         * @param parts The target, as an array of names of each element along the path
         * @return A Reference set to the given path
         */
        public static GddlReference Relative(params string[] parts)
        {
            return new GddlReference(false, parts);
        }

        public override bool IsReference => true;
        public override GddlReference AsReference => this;

        public bool Rooted { get; }

        public override bool IsResolved => resolved;
        public override GddlElement ResolvedValue => resolvedValue;

        public GddlReference(bool rooted, params string[] parts)
        {
            Rooted = rooted;
            nameParts.AddRange(parts);
        }

        /**
         * @return The current path of this reference
         */
        public IList<string> NameParts => nameParts.AsReadOnly();

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

        #region Implementation
        private readonly List<string> nameParts = new List<string>();

        private bool resolved;
        private GddlElement resolvedValue;
        #endregion

        #region Element

        public override GddlReference CopyInternal()
        {
            var reference = new GddlReference(Rooted);
            CopyTo(reference);
            return reference;
        }

        protected override void CopyTo(GddlReference other)
        {
            base.CopyTo(other);
            other.AddRange(nameParts);
        }

        public override void Resolve(GddlElement root)
        {
            if (IsResolved)
                return;

            resolved = TryResolve(root, !Rooted);
        }

        private bool TryResolve(GddlElement root, bool relative)
        {
            var parent = Parent;

            GddlElement target;
            if (relative)
            {
                target = parent;
                if (target == null) // In case this element is itself the root.
                    target = this;
            }
            else
            {
                target = root;
            }

            bool parentRoot = false;

            if (target.Parent != null)
            {
                var targetParent = target.Parent;
                if (targetParent.IsMap)
                {
                    parentRoot = targetParent.AsMap.KeysOf(target).Any(key => Equals(key,nameParts[0]));
                }
            }

            for (int i = parentRoot ? 1 : 0; i < nameParts.Count; i++)
            {
                string part = nameParts[i];

                if (!target.IsMap)
                    continue;

                var s = target.AsMap;

                if (s.TryGetValue(part, out var ne))
                {
                    target = ne;
                    continue;
                }

                resolvedValue = null;
                return false;
            }

            if (!target.IsResolved)
                target.Resolve(root);

            resolvedValue = target.ResolvedValue;

            if (ReferenceEquals(resolvedValue, this))
                throw new InvalidOperationException("Invalid cyclic reference: Reference resolves to itself.");

            while (parent != null)
            {
                if (ReferenceEquals(resolvedValue, parent))
                    throw new InvalidOperationException("Invalid cyclic reference: Reference resolves to a parent of the current element.");
                parent = parent.Parent;
            }

            return resolvedValue != null;
        }

        public override GddlElement Simplify()
        {
            if (resolved && resolvedValue != null)
                return resolvedValue.Copy();

            return this;

        }
        #endregion

        #region Equality

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((GddlReference)other);
        }

        public override bool Equals(GddlReference other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(GddlReference other)
        {
            return base.EqualsImpl(other) &&
                Rooted == other.Rooted &&
                Utility.ListEquals(nameParts, other.nameParts) &&
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