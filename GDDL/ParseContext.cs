
namespace GDDL
{
    public class ParseContext
    {
        public string Filename;
        public int Line;
        public int Column;

        public ParseContext(string f, int l, int c)
        {
            Filename = f;
            Line = l;
            Column = c;
        }

        public override string ToString()
        {
            return string.Format("{0}({1},{2})", Filename, Line, Column);
        }
    }
}
