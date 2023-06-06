using System;
using System.Collections.Generic;
using System.IO;

namespace CodeAnalysisApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Example1.DoIt();

            // 指定のディレクトリーの中のファイル一覧
            // 📖 [ディレクトリ内にあるファイルの一覧を取得する [C#]](https://johobase.com/get-files-csharp/)
            
            // C# ファイルへのパス一覧
            List<string> csharpFilePathList = new List<string>();
            //// ファイルパスのリスト
            //List<string> csharpFilePathList = new List<string>
            //{
            //    "C:\\Users\\むずでょ\\Documents\\Unity Projects\\RMU-1-00-00-Research-Project\\Assets\\RPGMaker\\Codebase\\CoreSystem\\Knowledge\\JsonStructure\\ChapterJson.cs",
            //};

            {
                // ディレクトリパス
                string dirPath = @"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research-Project\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\JsonStructure";

                // ディレクトリ直下のすべてのファイル一覧を取得する
                string[] allCsharpFiles = Directory.GetFiles(dirPath, "*.cs");
                foreach (string csharpFile in allCsharpFiles)
                {
                    csharpFilePathList.Add(csharpFile);
                }
            }

            foreach (var csharpFilePath in csharpFilePathList)
            {
                Example2.DoIt(csharpFilePath);
            }
        }
    }
}
