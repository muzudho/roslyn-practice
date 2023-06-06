using System.Collections.Generic;

namespace CodeAnalysisApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Example1.DoIt();

            // ファイルパスのリスト
            List<string> filePathList = new List<string>
            {
                "C:\\Users\\むずでょ\\Documents\\Unity Projects\\RMU-1-00-00-Research-Project\\Assets\\RPGMaker\\Codebase\\CoreSystem\\Knowledge\\JsonStructure\\ChapterJson.cs",
            };

            foreach (var filePath in filePathList)
            {
                Example2.DoIt(filePath);
            }
        }
    }
}
