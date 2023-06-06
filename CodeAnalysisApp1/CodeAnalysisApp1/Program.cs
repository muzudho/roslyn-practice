using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysisApp1
{
    internal class Program
    {
        static string ReadTextFile()
        {
            // ファイルパス
            var filePath = "C:\\Users\\むずでょ\\Documents\\Unity Projects\\RMU-1-00-00-Research-Project\\Assets\\RPGMaker\\Codebase\\CoreSystem\\Knowledge\\JsonStructure\\ChapterJson.cs";

            // テキスト・ファイル読込
            return File.ReadAllText(filePath);
        }

        static string GetExample()
        {
            return @"using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}";
        }

        static async Task Main(string[] args)
        {
            // 読込対象のテキスト
            string programText = ReadTextFile();
            // string programText = GetExample();

            //
            // テキストをパースして、ツリー作成
            //
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);

            //
            // ツリーから根を取得
            //
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            //
            // 根のデータ
            //
            Console.WriteLine($"The tree is a {root.Kind()} node.");
            // The tree is a CompilationUnit node.

            //
            // 根の要素数
            //
            Console.WriteLine($"The tree has {root.Members.Count} elements in it.");
            // The tree has 1 elements in it.

            //
            // Using 句解析
            //
            Console.WriteLine(StringifyUsingStatements(root));

            //
            // 根にぶら下がっている最初のものの種類
            //
            MemberDeclarationSyntax firstMember = root.Members[0];
            Console.WriteLine($"The first member is a {firstMember.Kind()}.");
            // The first member is a NamespaceDeclaration.

            var helloWorldDeclaration = (NamespaceDeclarationSyntax)firstMember;

            //
            var programDeclaration = (ClassDeclarationSyntax)helloWorldDeclaration.Members[0];
            //
            // そのメソッド数
            Console.WriteLine($"There are {programDeclaration.Members.Count} members declared in the {programDeclaration.Identifier} class.");
            // There are 1 members declared in the Program class.
            // There are 13 members declared in the ChapterJson class.

            Console.WriteLine($"The first member is a {programDeclaration.Members[0].Kind()}.");
            // The first member is a MethodDeclaration.
            // The first member is a FieldDeclaration.

            switch (programDeclaration.Members[0].Kind())
            {
                // メソッドなら
                case SyntaxKind.MethodDeclaration:
                    {
                        //
                        // プログラム中の宣言メンバーの１つ目
                        //
                        var methodDeclaration = (MethodDeclarationSyntax)programDeclaration.Members[0];

                        //
                        // 返却値の型
                        //
                        Console.WriteLine($"The return type of the {methodDeclaration.Identifier} method is {methodDeclaration.ReturnType}.");
                        // The return type of the Main method is void.

                        //
                        // 引数の個数
                        //
                        Console.WriteLine($"The method has {methodDeclaration.ParameterList.Parameters.Count} parameters.");
                        // The method has 1 parameters.

                        //
                        // 各引数
                        //
                        foreach (ParameterSyntax item in methodDeclaration.ParameterList.Parameters)
                        {
                            //
                            // 引数１つ分
                            //
                            Console.WriteLine($"The type of the {item.Identifier} parameter is {item.Type}.");
                            // The type of the args parameter is string[].
                        }

                        //
                        // 関数定義文
                        //
                        Console.WriteLine($"The body text of the {methodDeclaration.Identifier} method follows:");
                        Console.WriteLine(methodDeclaration.Body?.ToFullString());
                        // {
                        //     Console.WriteLine("Hello, World!");
                        // }

                        var argsParameter = methodDeclaration.ParameterList.Parameters[0];
                    }
                    break;

                // フィールドの宣言部なら
                case SyntaxKind.FieldDeclaration:
                    {
                        //
                        // プログラム中の宣言メンバーの１つ目
                        //
                        var fieldDeclaration = (FieldDeclarationSyntax)programDeclaration.Members[0];

                        Console.WriteLine($"fullString: {fieldDeclaration.ToFullString()}.");
                        //            fullString:         /// <summary>
                        //                                /// ?? 章Idの前に
                        //                                /// </summary>
                        //public int beforeChapterId;

                        // コメント、アクセス修飾子、戻り値の型、名前はありそうだが

                        Console.WriteLine($"AttributeLists: {fieldDeclaration.AttributeLists}");
                        // AttributeLists:

                        Console.WriteLine($"ContainsAnnotations: {fieldDeclaration.ContainsAnnotations}");
                        // ContainsAnnotations: False

                        Console.WriteLine($"ContainsDiagnostics: {fieldDeclaration.ContainsDiagnostics}");
                        // ContainsDiagnostics: False

                        Console.WriteLine($"ContainsDirectives:  {fieldDeclaration.ContainsDirectives}");
                        // ContainsDirectives:  False

                        Console.WriteLine($"ContainsSkippedText: {fieldDeclaration.ContainsSkippedText}");
                        // ContainsSkippedText: False

                        Console.WriteLine($"Declaration:         {fieldDeclaration.Declaration}");
                        // Declaration:         int beforeChapterId

                        Console.WriteLine($"FullSpan:            {fieldDeclaration.FullSpan}");
                        // FullSpan:            [297..404)

                        Console.WriteLine($"HasLeadingTrivia:    {fieldDeclaration.HasLeadingTrivia}");
                        // HasLeadingTrivia:    True

                        Console.WriteLine($"HasStructuredTrivia: {fieldDeclaration.HasStructuredTrivia}");
                        // HasStructuredTrivia: True

                        Console.WriteLine($"HasTrailingTrivia:   {fieldDeclaration.HasTrailingTrivia}");
                        // HasTrailingTrivia:   True

                        Console.WriteLine($"IsMissing:           {fieldDeclaration.IsMissing}");
                        // IsMissing:           False

                        Console.WriteLine($"IsStructuredTrivia:  {fieldDeclaration.IsStructuredTrivia}");
                        // IsStructuredTrivia:  False

                        Console.WriteLine($"Language:            {fieldDeclaration.Language}");
                        // Language:            C#

                        Console.WriteLine($"Modifiers:           {fieldDeclaration.Modifiers}");
                        // Modifiers:           public

                        Console.WriteLine($"Parent:              {fieldDeclaration.Parent}");
                        // Parent:              [Serializable]
                        //public class ChapterJson : IJsonStructure
                        //    {
                        //        /// <summary>
                        //        /// ?? 章Idの前に
                        //        /// </summary>
                        //        public int beforeChapterId;
                        // 以下略

                        Console.WriteLine($"ParentTrivia:        {fieldDeclaration.ParentTrivia}");
                        // ParentTrivia:

                        Console.WriteLine($"RawKind:             {fieldDeclaration.RawKind}");
                        // RawKind:             8873

                        Console.WriteLine($"SemicolonToken:      {fieldDeclaration.SemicolonToken}");
                        // SemicolonToken:      ;

                        Console.WriteLine($"Span:                {fieldDeclaration.Span}");
                        // Span:                [375..402)

                        Console.WriteLine($"SpanStart:           {fieldDeclaration.SpanStart}");
                        // SpanStart:           375

                        Console.WriteLine($"SyntaxTree:          {fieldDeclaration.SyntaxTree}");
                        // SyntaxTree:          using System;
                        //                        using System.Collections.Generic;
                        //
                        ///// <summary>
                        ///// ?? JSON構造体
                        ///// </summary>
                        //namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
                        //    {
                        //        /// <summary>
                        //        /// ?? 章JSON
                        //        /// </summary>
                        //        [Serializable]
                        //        public class ChapterJson : IJsonStructure
                        //        {
                        //            /// <summary>
                        //            /// ?? 章Idの前に
                        //            /// </summary>
                        //            public int beforeChapterId;

                        //
                        // ドキュメント・コメントをうまく取る方法が無くないか？
                        //
                    }
                    break;
            }
        }

        /// <summary>
        /// Using 句解析
        /// </summary>
        /// <param name="root">根要素</param>
        /// <returns>結果文字列</returns>
        static string StringifyUsingStatements(CompilationUnitSyntax root)
        {
            var builder = new StringBuilder();

            //
            // Using 句の数
            //
            builder.AppendLine($"The tree has {root.Usings.Count} using statements. They are:");
            // The tree has 4 using statements.They are:

            // 根にぶら下がっている Using 句を出力
            foreach (UsingDirectiveSyntax element in root.Usings)
            {
                builder.AppendLine($"\t{element.Name}");
            }
            // System
            // System.Collections
            // System.Linq
            // System.Text

            return builder.ToString();
        }
    }
}
