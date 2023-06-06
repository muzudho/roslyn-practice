using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace CodeAnalysisApp1
{
    internal class Example2
    {
        internal static void DoIt(
            string filePath)
        {
            // コンソールに出力すると文字化けするのは、コンソールの方のエンコーディング設定（コードページ）が悪い
            // 📖 [How to get CMD/console encoding in C#](https://stackoverflow.com/questions/5910573/how-to-get-cmd-console-encoding-in-c-sharp)
            Console.OutputEncoding = Encoding.UTF8; // これでも絵文字は表示されない

            // 読込対象のテキスト
            string programText = File.ReadAllText(filePath, Encoding.UTF8);

            //
            // テキストをパースして、ツリー作成
            // ツリーから根を取得
            // 根にぶら下がっている最初のものの種類
            //
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
                text: programText,
                encoding: Encoding.UTF8);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            foreach (var rootMember in root.Members)
            {
                var helloWorldDeclaration = (NamespaceDeclarationSyntax)rootMember;
                var programDeclaration = (ClassDeclarationSyntax)helloWorldDeclaration.Members[0];
                var programDeclarationMember = programDeclaration.Members[0];

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

                            // コメント、アクセス修飾子、戻り値の型、名前はありそうだが
                            var (declaration, modifiers, summary) = ParseField(fieldDeclaration);
                            Console.WriteLine($"{declaration},{modifiers},{summary}");

                        }
                        break;
                }
            }
        }

        static (string, string, string) ParseField(FieldDeclarationSyntax fieldDeclaration)
        {
            var declaration = fieldDeclaration.Declaration;
            // Declaration:         int beforeChapterId

            var modifiers = fieldDeclaration.Modifiers;
            // Modifiers:           public

            var leadingTrivia = fieldDeclaration.GetLeadingTrivia();
            //leadingTrivia:         /// <summary>
            //                       /// ?? 章Idの前に
            //                       /// </summary>

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

            summaryText = summaryText.Replace("\r\n", "\\n");

            return (declaration.ToString(), modifiers.ToString(), summaryText);
        }
    }
}
