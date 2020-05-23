using System;
using System.Collections.Generic;
using System.Text;
using GDDL.Config;

namespace GDDL.Structure
{
    public class Reference : Element
    {
        protected readonly List<string> NamePart = new List<string>();

        private bool resolved;
        private Element resolvedValue;

        public bool Rooted { get; set; }

        public override bool IsResolved => resolved;
        public override Element ResolvedValue => resolvedValue;

        internal Reference(params string[] parts)
        {
            NamePart.AddRange(parts);
        }

        internal Reference(bool rooted, params string[] parts)
        {
            Rooted = rooted;
            NamePart.AddRange(parts);
        }

        public void Add(string name)
        {
            NamePart.Add(name);
        }

        public void AddRange(IEnumerable<string> names)
        {
            NamePart.AddRange(names);
        }

        public override Element Copy()
        {
            var b = new Reference();
            CopyTo(b);
            return b;
        }

        protected override void CopyTo(Element other)
        {
            base.CopyTo(other);
            if (!(other is Reference))
                throw new ArgumentException("CopyTo for invalid type", nameof(other));
            var b = (Reference)other;
            b.AddRange(NamePart);
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

            bool parentRoot = parent.HasName() && NamePart[0] == parent.Name;

            for (int i = parentRoot ? 1 : 0; i < NamePart.Count; i++)
            {
                string part = NamePart[0];

                var s = elm as Collection;

                if (s == null)
                    continue;

                Element ne;
                if (s.TryGetValue(part, out ne))
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

        protected override void ToStringImpl(StringBuilder builder, StringGenerationContext ctx)
        {
            int count = 0;
            foreach (var it in NamePart)
            {
                if (count++ > 0)
                    builder.Append(':');
                builder.Append(it);
            }

            if (IsResolved)
            {
                builder.Append('=');
                if (ResolvedValue == null)
                    builder.Append("NULL");
                else
                    builder.Append(ResolvedValue);
            }
        }
    }
}