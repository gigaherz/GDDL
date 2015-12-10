using System.IO;

namespace GDDL.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = Parser.FromFile("Samples\\Test.txt");
            var parsedData = p.Parse();
            File.WriteAllText("Output.txt", parsedData.ToString(new StringGenerationContext(StringGenerationOptions.Nice)));
        }
    }
}
