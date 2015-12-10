using System.IO;

namespace GDDL.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = Parser.FromFile("H:\\Projects\\ProjectBlob2\\Dawnflare\\Output\\Data\\Scenes\\MainMenu.scene");
            var parsedData = p.Parse();
            File.WriteAllText("F:\\gen.txt", parsedData.ToString(new StringGenerationContext(StringGenerationOptions.Nice)));
        }
    }
}
