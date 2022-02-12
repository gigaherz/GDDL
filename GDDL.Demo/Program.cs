using System;
using GDDL.Serialization;

namespace GDDL.Demo
{
    public static class Program
    {
        public static void Main()
        {
            var p = Gddl.FromFile("Test.txt");
            var parsedData = p.Parse();
            var text = Formatter.FormatNice(parsedData);
            //File.WriteAllText("Output.txt", text);

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Write(text);
            Console.Out.Flush();
        }
    }
}
