namespace CodeAnalysisApp1
{
    using CodeAnalysisApp1.Example2;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using TheExample2 = CodeAnalysisApp1.Example2;

    internal class Program
    {
        static void Main(string[] args)
        {
            // Example1.DoIt();

            //
            // 出力先ディレクトリーの準備
            //
            var targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "CodeAnalysisApp1");
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // 指定のディレクトリーの中のファイル一覧
            // 📖 [ディレクトリ内にあるファイルの一覧を取得する [C#]](https://johobase.com/get-files-csharp/)

            //// ファイルパスのリスト
            //List<string> csharpFilePathList = new List<string>
            //{
            //    "C:\\Users\\むずでょ\\Documents\\Unity Projects\\RMU-1-00-00-Research\\Assets\\RPGMaker\\Codebase\\CoreSystem\\Knowledge\\JsonStructure\\ChapterJson.cs",
            //};

            //*
            //
            // 出力先CSVファイル名と、読取元ディレクトリー・パスのリストの辞書
            // ================================================
            //
            var directoryMap = new Dictionary<string, List<string>>()
            {
                // RMU 1-00-00 Research
                {"😁Data Map.csv", new List<string>(){
                                                                @"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Add-ons",
                                                                @"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem",
                                                                @"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime",
                                                                @"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Editor",

                                                                ////
                                                                //// CoreSystem
                                                                ////

                                                                //// Data Model
                                                                //// このフォルダーは、サブ・フォルダーを無駄に作ってる
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Animation",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Armor",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\AssetManage",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\CharacterActor",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Class",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Common",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Encounter",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Enemy",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Event",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\EventBattle",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\EventCommon",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\EventMap",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Flag",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Item",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Map",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Outline",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Runtime",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\SkillCommon",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\SkillCustom",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Sound",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\State",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\SystemSetting",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Troop",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\UiSetting",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Vehicle",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\Weapon",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\DataModel\WordDefinition",
                                                                //// Enum
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\Enum",
                                                                //// Json Structure
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\JsonStructure",
                                                                //// Misc
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Knowledge\Misc",

                                                                ////
                                                                //// Service/*/Repository
                                                                ////
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Service\DatabaseManagement\Repository",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Service\EventManagement\Repository",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Service\MapManagement\Repository",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Service\OutlineManagement\Repository",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\CoreSystem\Service\RuntimeDataManagement\Repository",

                                                                ////
                                                                //// Runtime
                                                                ////
                                                                //// TODO データとか区別なく、とっちらかってる。結局、全部サーチすりゃいいんじゃないか？
                                                                ////

                                                                //// Addon/Knowledge
                                                                //// Addon/Service/Repository
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Addon\Knowledge",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Addon\Service\Repository",

                                                                //// Battle
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Battle\Sprites",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Battle\Window",

                                                                //// Common
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Common\Component\Hud\Actor",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Common\Component\Hud\Character",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Common\Component\Hud\Display",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Common\Component\Hud\GameProgress",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Common\Component\Hud\Message",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Common\Component\Hud\Party",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Common\Enum",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Common",

                                                                //// GameState
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\GameState",

                                                                //// Map コメント打ち込んでないの　いっぱい残ってる
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Map\Component\Character",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Map\Component\Map",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Map\Component",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Map\InputName",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Map\Item",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Map\Menu",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Map\Shop",
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Map",


                                                                //// Title
                                                                //@"C:\Users\むずでょ\Documents\Unity Projects\RMU-1-00-00-Research\Assets\RPGMaker\Codebase\Runtime\Title",
                                                            }
                },
            };
            // */

            //
            // 出力先CSVファイル名と、読取元ファイル・パスのリストの辞書
            // =========================================================
            //
            var fileMap = new Dictionary<string, List<string>>();

            //*
            {

                foreach (var entry in directoryMap)
                {
                    var filePathToSave = entry.Key;
                    var folderPathListToRead = entry.Value;

                    //
                    // マージ
                    //
                    foreach (var folderPathToRead in folderPathListToRead)
                    {
                        // ディレクトリ直下のすべてのファイル一覧を取得する
                        // // string[] allCsharpFiles = Directory.GetFiles(folderPathToRead, "*.cs");

                        // サブ・ディレクトリまで調べる
                        string[] allCsharpFiles = Directory.GetFiles(folderPathToRead, "*.cs", SearchOption.AllDirectories);
                        foreach (string csharpFile in allCsharpFiles)
                        {
                            if (!fileMap.ContainsKey(filePathToSave))
                            {
                                fileMap.Add(filePathToSave, new List<string>() { csharpFile });
                            }
                            else
                            {
                                fileMap[filePathToSave].Add(csharpFile);
                            }
                        }
                    }
                }
            }
            // */

            foreach (var entry1 in fileMap)
            {
                var filePathToSave = entry1.Key;
                var filePathListToRead = entry1.Value;

                //
                // 解析とマージ
                //
                var recordExList = new List<RecordEx>();

                foreach (var filePathToRead in filePathListToRead)
                {
                    TheExample2.Example2.DoIt(
                        filePathToRead: filePathToRead,
                        setRecordExList: (subRecordExList) =>
                        {
                            recordExList.AddRange(subRecordExList);
                        });
                }

                //
                // CSV書出し
                //
                var builder = new StringBuilder();
                // ヘッダー CSV
                builder.AppendLine("FilePlace,CodePlace,Access,MemberType,Name,Value,Summary");
                // データ行
                foreach (var recordEx in recordExList)
                {
                    // CSV
                    builder.AppendLine(recordEx.ToCSV());
                }

                var csvContent = builder.ToString();
                Console.WriteLine($@"Write to: {filePathToSave}
{csvContent}");

                //
                // ファイルへの書き出し
                //
                var savePath = Path.Combine(targetDirectory, filePathToSave);
                File.WriteAllText(savePath, csvContent, Encoding.UTF8);

            }
        }
    }
}
