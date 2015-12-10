namespace GDDL.Structure
{
    public class TypedSet : Set
    {
        private readonly string name;

        public string TypeName
        {
            get { return name; }
        }

        internal TypedSet(string name)
        {
            this.name = name;
        }

        internal TypedSet(string name, Set s)
        {
            this.name = name;
            AddRange(s.Contents);
        }

        public override Element Simplify()
        {
            base.Simplify();
            return this;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", name, base.ToString());
        }

        public override string ToString(StringGenerationContext ctx)
        {
            return string.Format("{0} {1}", name, base.ToString(ctx));
        }
    }
}