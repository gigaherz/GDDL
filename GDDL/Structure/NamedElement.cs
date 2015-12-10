namespace GDDL.Structure
{
    public class NamedElement : Element, INamed
    {
        private string name;
        private Element value;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Element Value
        {
            get { return value; }
            set { this.value = value; }
        }

        internal NamedElement(string id, Element el)
        {
            name = id;
            value = el;
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", name, value);
        }

        public override string ToString(StringGenerationContext ctx)
        {
            return string.Format("{0} = {1}", name, value.ToString(ctx));
        }

        internal override void Resolve(Set rootSet)
        {
            value.Resolve(rootSet);
        }

        public override bool IsResolved
        {
            get { return value.IsResolved; }
        }

        public override Element ResolvedValue
        {
            get { return value.ResolvedValue; }
        }
    }
}