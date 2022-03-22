using System;
using System.Linq;
using GDDL.Serialization;

namespace GDDL.Demo
{
    public static class Program
    {
        public static void Main()
        {
            var doc = Gddl.FromFile("Test.txt");
            var text = Formatter.Format(doc, FormatterOptions.Nice);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Write(text);
            Console.Out.Flush();
            //File.WriteAllText("Output.txt", text);

            var v2 = doc.Root["named list"][1][0];
            Console.WriteLine("[" + string.Join(",", v2) + "]");

            var v3 = doc.Root.Query("'named list'/[1]/[0]");
            Console.WriteLine("[" + string.Join(",", v3) + "]");
        }
    }
}
