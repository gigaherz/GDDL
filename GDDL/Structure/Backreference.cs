using System.Collections.Generic;
using System.Text;

namespace GDDL.Structure
{
    public class Backreference : Element
    {
        protected readonly List<string> NamePart = new List<string>();

        // TODO: Figure out what this syntax feature was meant to be used for XD
        protected bool Rooted;

        private bool resolved;
        private Element resolvedValue;

        public override Element ResolvedValue
        {
            get { return resolvedValue; }
        }

        public override bool IsResolved
        {
            get { return resolved; }
        }

        internal Backreference(bool rooted, string I)
        {
            Rooted = rooted;
            NamePart.Add(I);
        }

        internal virtual void Append(string I)
        {
            NamePart.Add(I);
        }

        protected override string ToStringInternal()
        {
            var ss = new StringBuilder();
            int count = 0;
            foreach (var it in NamePart)
            {
                if (count++ > 0)
                    ss.Append(':');
                ss.Append(it);
            }

            if (IsResolved)
            {
                ss.Append('=');
                if (ResolvedValue == null)
                    ss.Append("NULL");
                else
                    ss.Append(ResolvedValue);
            }

            return ss.ToString();
        }

        protected override string ToStringInternal(StringGenerationContext ctx)
        {
            return ToStringInternal();
        }

        internal override void Resolve(Element root)
        {
            if (IsResolved)
                return;

            Element elm = root;

            foreach (var it in NamePart)
            {
                var s = elm as Set;

                if (s == null)
                    continue;

                Element ne;
                if (s.TryGetByName(it, out ne))
                {
                    elm = ne;
                    continue;
                }

                resolvedValue = null;
                resolved = true;
                return;
            }

            resolvedValue = elm;
            resolved = true;

            if (!elm.IsResolved)
                elm.Resolve(root);

            resolvedValue = elm.ResolvedValue;
        }

        public override Element Simplify()
        {
            if (resolved && resolvedValue != null)
                return resolvedValue;

            return this;
        }
    }
}