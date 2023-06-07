namespace CodeAnalysisApp1.Example2
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    internal class Example2
    {
        internal static void DoIt(
            string filePathToRead,
            LazyCoding.SetValue<List<RecordEx>> setRecordExList)
        {
            // コンソールに出力すると文字化けするのは、コンソールの方のエンコーディング設定（コードページ）が悪い
            // 📖 [How to get CMD/console encoding in C#](https://stackoverflow.com/questions/5910573/how-to-get-cmd-console-encoding-in-c-sharp)
            Console.OutputEncoding = Encoding.UTF8; // これでも絵文字は表示されない

            // 読込対象のテキスト
            string programText = File.ReadAllText(filePathToRead, Encoding.UTF8);

            //
            // テキストをパースして、ツリー作成
            // ツリーから根を取得
            // 根にぶら下がっている最初のものの種類
            //
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
                text: programText,
                encoding: Encoding.UTF8);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            var recordExList = new List<RecordEx>();

            foreach (var rootMember in root.Members)
            {
                var namespaceDeclaration = (NamespaceDeclarationSyntax)rootMember;

                // クラスが２個定義されてるとか、列挙型が定義されてるとかに対応
                foreach (var memberDeclaration in namespaceDeclaration.Members)
                {
                    switch (memberDeclaration.Kind())
                    {
                        case SyntaxKind.ClassDeclaration:
                            {
                                ParseClassDeclaration(
                                    setRecord: (record) =>
                                    {
                                        recordExList.Add(new RecordEx(
                                            recordObj: record,
                                            filePathToRead: filePathToRead));
                                    },
                                    @namespace: namespaceDeclaration.Name.ToString(),
                                    programDeclaration: (ClassDeclarationSyntax)memberDeclaration);
                            }
                            break;

                        case SyntaxKind.InterfaceDeclaration:
                            {
                                var programDeclaration = (InterfaceDeclarationSyntax)memberDeclaration;

                                foreach (var programDeclarationMember in programDeclaration.Members)
                                {
                                    switch (programDeclarationMember.Kind())
                                    {
                                        // フィールドの宣言部なら
                                        case SyntaxKind.FieldDeclaration:
                                            {
                                                //
                                                // プログラム中の宣言メンバーの１つ目
                                                //
                                                var fieldDeclaration = (FieldDeclarationSyntax)programDeclarationMember;
                                                //            fullString:         /// <summary>
                                                //                                /// ?? 章Idの前に
                                                //                                /// </summary>
                                                //public int beforeChapterId;

                                                var record = ParseField(
                                                    fieldDeclaration: fieldDeclaration,
                                                    @namespace: namespaceDeclaration.Name.ToString());
                                                recordExList.Add(new RecordEx(
                                                    recordObj: record,
                                                    filePathToRead: filePathToRead));
                                            }
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }
                            break;

                        case SyntaxKind.EnumDeclaration:
                            {
                                ParseEnumDeclaration(
                                    setRecord: (record) =>
                                    {
                                        recordExList.Add(new RecordEx(
                                            recordObj: record,
                                            filePathToRead: filePathToRead));
                                    },
                                    @namespace: string.Empty,
                                    programDeclaration: (EnumDeclarationSyntax)memberDeclaration);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            setRecordExList(recordExList);
        }

        /// <summary>
        /// class 型の定義を解析
        /// </summary>
        static void ParseClassDeclaration(LazyCoding.SetValue<Record> setRecord, string @namespace, ClassDeclarationSyntax programDeclaration)
        {
            // サブ・クラスが２個定義されてるとか、サブ・列挙型が定義されてるとかに対応
            foreach (var programDeclarationMember in programDeclaration.Members)
            {
                switch (programDeclarationMember.Kind())
                {
                    // フィールドの宣言部なら
                    case SyntaxKind.FieldDeclaration:
                        {
                            //
                            // プログラム中の宣言メンバーの１つ目
                            //
                            var fieldDeclaration = (FieldDeclarationSyntax)programDeclarationMember;
                            //            fullString:         /// <summary>
                            //                                /// ?? 章Idの前に
                            //                                /// </summary>
                            //public int beforeChapterId;

                            var record = ParseField(
                                fieldDeclaration: fieldDeclaration,
                                // ネームスペース.親クラス名　とつなげる
                                @namespace: $"{@namespace}.{programDeclaration.Identifier.ToString()}");
                            setRecord(record);
                        }
                        break;


                    // サブ列挙型
                    case SyntaxKind.EnumDeclaration:
                        {
                            ParseEnumDeclaration(
                                setRecord: setRecord,
                                // ネームスペース.親クラス名.自列挙型名　とつなげる
                                @namespace: $"{@namespace}.{programDeclaration.Identifier.ToString()}.{((EnumDeclarationSyntax)programDeclarationMember).Identifier}",
                                programDeclaration: (EnumDeclarationSyntax)programDeclarationMember);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 列挙型の定義を解析
        /// </summary>
        static void ParseEnumDeclaration(LazyCoding.SetValue<Record> setRecord, string @namespace, EnumDeclarationSyntax programDeclaration)
        {
            foreach (var programDeclarationMember in programDeclaration.Members)
            {
                switch (programDeclarationMember.Kind())
                {
                    // フィールドの宣言部なら
                    case SyntaxKind.EnumMemberDeclaration:
                        {
                            //
                            // プログラム中の宣言メンバーの１つ目
                            //
                            var fieldDeclaration = programDeclarationMember;

                            var record = ParseField(fieldDeclaration, @namespace);
                            setRecord(record);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// class, interface のフィールド用
        /// 
        /// - プロパティにもヒットする
        /// </summary>
        /// <param name="fieldDeclaration"></param>
        /// <returns></returns>
        static Record ParseField(FieldDeclarationSyntax fieldDeclaration, string @namespace)
        {
            //
            // モディファイア
            // ==============
            //
            var modifiers = fieldDeclaration.Modifiers;
            // Modifiers:           public

            //
            // デクラレーション
            // ================
            //
            string declarationHeadText;
            string name;
            string value;
            if (fieldDeclaration.Declaration != null)
            {
                // Declaration:         int beforeChapterId
                //
                // List<string> AttackMotionImageLabel = new List<string> { "無し", "ダガー", "剣", "フレイル", "斧", "ウィップ", "杖", "弓", "クロスボウ", "銃", "爪", "グローブ", "槍", "メイス", "ロッド", "こん棒", "チェーン", "未来の剣", "パイプ", "ショットガン", "ライフル", "チェーンソー", "レールガン", "スタンロッド", "ユーザ定義1", "ユーザ定義2", "ユーザ定義3", "ユーザ定義4", "ユーザ定義5", "ユーザ定義6" }

                // 連続する空白を１つにしてみる
                // 📖 [Replace consecutive whitespace characters with a single space in C#](https://www.techiedelight.com/replace-consecutive-whitespace-by-single-space-csharp/)
                // var declarationText = Regex.Replace(fieldDeclaration.Declaration.ToString(), @"\s+", " ");
                var declarationText = Regex.Replace(fieldDeclaration.Declaration.ToString(), @"\s+", " ");

                // "=" を含むか？
                if (declarationText.Contains("="))
                {
                    // "=" より前だけ取るか
                    var tokenList = declarationText.Split('=').ToList();

                    declarationText = tokenList[0].TrimEnd();
                    tokenList.RemoveAt(0);
                    value = string.Join("=", tokenList);
                }
                else
                {
                    value = string.Empty;
                }

                // とりあえず半角スペースで区切ってみるか
                string[] list = declarationText.ToString().Split(' ');

                var declarationHead = new string[list.Length - 1];
                Array.Copy(list, 0, declarationHead, 0, list.Length - 1);
                declarationHeadText = string.Join(" ", declarationHead);
                name = list[list.Length - 1];
            }
            else
            {
                declarationHeadText = string.Empty;
                name = string.Empty;
                value = string.Empty;
            }

            //
            // 前トリビア
            // ==========
            //
            var leadingTrivia = fieldDeclaration.GetLeadingTrivia();
            //leadingTrivia:         /// <summary>
            //                       /// ?? 章Idの前に
            //                       /// </summary>

            var documentCommentBuilder = new StringBuilder();
            var documentComment = leadingTrivia.ToFullString();
            // 改行は必ず `\r\n` （CRLF） とすること
            var documentCommentLines = documentComment.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (var line in documentCommentLines)
            {
                var match = Regex.Match(line, @"\s*/// ?(.*)");
                if (match.Success)
                {
                    var content = match.Groups[1];
                    documentCommentBuilder.AppendLine(content.ToString());
                }
            }
            var documentCommentText = documentCommentBuilder.ToString();
            //documentCommentText: < summary >
            //?? 章Idの前に
            //</ summary >

            //
            // XMLパーサーが欲しい
            //
            // 📖 [How do I read and parse an XML file in C#?](https://stackoverflow.com/questions/642293/how-do-i-read-and-parse-an-xml-file-in-c)
            //
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(documentCommentText);

            XmlNode summaryNode = doc.DocumentElement.SelectSingleNode("/summary");
            string summaryText = summaryNode.InnerText;
            //                    summaryText:
            //?? 章Idの前に

            summaryText = summaryText.Replace("\r\n", "\\r\\n");

            return new Record(
                type: @namespace,
                access: modifiers.ToString(),
                memberType: declarationHeadText,
                name: name,
                value: value,
                summary: summaryText);
        }

        /// <summary>
        /// enum 型のメンバー用
        /// </summary>
        /// <param name="enumMemberDeclaration"></param>
        /// <returns></returns>
        static Record ParseField(EnumMemberDeclarationSyntax enumMemberDeclaration, string @namespace)
        {
            var modifiers = enumMemberDeclaration.Modifiers;
            // Modifiers:           public

            var identifierText = enumMemberDeclaration.Identifier.ToString();

            var leadingTrivia = enumMemberDeclaration.GetLeadingTrivia();
            //leadingTrivia:         /// <summary>
            //                       /// ?? 章Idの前に
            //                       /// </summary>

            //
            // Enum 値
            //
            // 📖 [Roslyn CodeAnalysisでenumの値を取得したい](https://teratail.com/questions/290108?sort=1)
            // 静的には、取れないようだ？
            //
            // `= 値` が書かれているかどうかは取得できるようだ？
            //
            string enumValue;
            if (enumMemberDeclaration.EqualsValue != null)
            {
                var equalsValueText = enumMemberDeclaration.EqualsValue.ToString();
                var match = Regex.Match(equalsValueText, @"=\s*(.*)");
                if (match.Success)
                {
                    enumValue = match.Groups[1].ToString();
                }
                else
                {
                    enumValue = equalsValueText;
                }
            }
            else
            {
                enumValue = string.Empty;
            }

            //
            // ドキュメント・コメント
            //
            var documentCommentBuilder = new StringBuilder();
            var documentComment = leadingTrivia.ToFullString();
            var documentCommentLines = documentComment.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (var line in documentCommentLines)
            {
                var match = Regex.Match(line, @"\s*/// ?(.*)");
                if (match.Success)
                {
                    var content = match.Groups[1];
                    documentCommentBuilder.AppendLine(content.ToString());
                }
            }
            var documentCommentText = documentCommentBuilder.ToString();

            //
            // XMLパーサーが欲しい
            //
            // 📖 [How do I read and parse an XML file in C#?](https://stackoverflow.com/questions/642293/how-do-i-read-and-parse-an-xml-file-in-c)
            //
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(documentCommentText);

            XmlNode summaryNode = doc.DocumentElement.SelectSingleNode("/summary");
            string summaryText = summaryNode.InnerText;
            //                    summaryText:
            //?? 章Idの前に

            summaryText = summaryText.Replace("\r\n", "\\r\\n");

            return new Record(
                type: @namespace,
                access: modifiers.ToString(),
                memberType: string.Empty,
                name: identifierText,
                value: enumValue,
                summary: summaryText);
        }
    }
}
