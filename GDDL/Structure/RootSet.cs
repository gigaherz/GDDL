namespace GDDL.Structure
{
    public class RootSet : Set
    {
        internal RootSet()
        {
        }

        internal RootSet(Set s)
        {
            AddRange(s.Contents);
        }

        public override Element Simplify()
        {
            base.Simplify();
            return this;
        }
    }
}