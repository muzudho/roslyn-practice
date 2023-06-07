namespace CodeAnalysisApp1
{
    using System.Collections.Generic;
    using System.IO;
    using TheExample2 = CodeAnalysisApp1.Example2;

    internal class Program
    {
        static void Main(string[] args)
        {
            // Example1.DoIt();

            // 指定のディレクトリーの中のファイル一覧
            // 📖 [ディレクトリ内にあるファイルの一覧を取得する [C#]](https://johobase.com/get-files-csharp/)

            //// ファイルパスのリスト
            //List<string> csharpFilePathList = new List<string>
            //{
            //    "C:\\Users\\むずでょ\\Documents\\Unity Projects\\RMU-1-00-00-Research-Project\\Assets\\RPGMaker\\Codebase\\CoreSystem\\Knowledge\\JsonStructure\\ChapterJson.cs",
            //};

            //*
            //
            // 出力先フォルダー名と、ディレクトリー・パスの辞書
            // ================================================
            //
            var directoryMap = new Dictionary<string, string>()
            {
                {"😁RMU 1.00.00 Research📂Assets📂RPGMaker📂Codebase📂CoreSystem📂Knowledge📂Enum", @"C:\\Users\\むずでょ\\Documents\\Unity Projects\\RMU-1-00-00-Research-Project\\Assets\\RPGMaker\\Codebase\\CoreSystem\\Knowledge\\Enum" },
                {"😁RMU 1.00.00 Research📂Assets📂RPGMaker📂Codebase📂CoreSystem📂Knowledge📂JsonStructure", @"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research-Project\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\JsonStructure" },
            };
            // */

            //
            // C# ファイルへのパス一覧
            // =======================
            //
            var targetFileDictionary = new Dictionary<string, string>();
            // ファイル・パスがキー
            // 保存先フォルダー名が値
            /*
            var targetFileDictionary = new Dictionary<string, string>()
            {
                {@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research-Project\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\Enum\BattleEnums.cs","😁RMU 1.00.00 Research📂Assets📂RPGMaker📂Codebase📂CoreSystem📂Knowledge📂Enum" },
            };
            */

            //*
            {

                foreach (var entry in directoryMap)
                {
                    // ディレクトリ直下のすべてのファイル一覧を取得する
                    string[] allCsharpFiles = Directory.GetFiles(entry.Value, "*.cs");
                    foreach (string csharpFile in allCsharpFiles)
                    {
                        targetFileDictionary.Add(csharpFile, entry.Key);
                    }
                }
            }
            // */

            foreach (var entry in targetFileDictionary)
            {
                TheExample2.Example2.DoIt(
                    readFilePath: entry.Key,
                    saveFolderName: entry.Value);
            }
        }
    }
}
